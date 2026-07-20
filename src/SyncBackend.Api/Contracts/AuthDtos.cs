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
