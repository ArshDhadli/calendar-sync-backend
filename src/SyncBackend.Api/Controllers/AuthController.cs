using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncBackend.Api.Auth;
using SyncBackend.Api.Contracts;
using SyncBackend.Api.Data;
using SyncBackend.Api.Entities;

namespace SyncBackend.Api.Controllers;

/// <summary>
/// Authentication endpoints: register an account, then log in to receive a JWT.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtTokenService _tokenService;

    // All three dependencies are supplied by the DI container configured in Program.cs.
    public AuthController(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        JwtTokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Create a new account. Rejects duplicate emails, stores only a hash of the
    /// password (never the plain text), and returns the new user's id.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        // Normalise so "Me@x.com" and "me@x.com" can't both register.
        var email = request.Email.Trim().ToLowerInvariant();

        // One account per email (also enforced by a unique index at the DB level).
        var emailTaken = await _db.Users.AnyAsync(u => u.Email == email);
        if (emailTaken)
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            CreatedAtUtc = DateTime.UtcNow,
        };

        // Hash the password. The hasher generates a random salt and runs a slow
        // PBKDF2 hash internally; the result string embeds the salt + iteration count.
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // 201 Created. We return only safe fields — never the hash.
        return CreatedAtAction(nameof(Register), new { id = user.Id }, new
        {
            id = user.Id,
            email = user.Email,
        });
    }

    /// <summary>
    /// Verify email + password and, on success, return a signed JWT. On any failure
    /// we return the same 401 with a generic message — never revealing whether it was
    /// the email or the password that was wrong (that would help credential-stuffing).
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Re-hash the submitted password with the stored hash's salt and compare.
        // VerifyHashedPassword does this in constant time to avoid timing attacks.
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Password is correct → mint the token that proves it on later requests.
        var token = _tokenService.CreateToken(user);
        return Ok(new AuthResponse { Token = token });
    }
}
