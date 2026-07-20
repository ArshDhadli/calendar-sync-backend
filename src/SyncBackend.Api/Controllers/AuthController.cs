using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncBackend.Api.Contracts;
using SyncBackend.Api.Data;
using SyncBackend.Api.Entities;

namespace SyncBackend.Api.Controllers;

/// <summary>
/// Authentication endpoints. Phase 1 starts with registration; login + JWT come next.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    // Both dependencies are supplied by the DI container we configured in Program.cs.
    public AuthController(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
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
}
