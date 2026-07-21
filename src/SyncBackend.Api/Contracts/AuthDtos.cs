using System.ComponentModel.DataAnnotations;

namespace SyncBackend.Api.Contracts;

/// <summary>
/// What a client sends to <c>POST /auth/register</c>. This is the API *contract* —
/// deliberately separate from the <see cref="Entities.User"/> entity so internal
/// fields (Id, PasswordHash) never leak into the API and the client can never set them.
/// The [Required]/[EmailAddress] attributes let ASP.NET validate the body automatically.
/// </summary>
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// What a client sends to <c>POST /auth/login</c>. No length rule here — we validate
/// credentials by looking them up, not by shape. (A short password just won't match.)
/// </summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// What login returns on success: the signed JWT the client stores and sends back
/// on every later request. Deliberately minimal — no user internals leak out.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
}
