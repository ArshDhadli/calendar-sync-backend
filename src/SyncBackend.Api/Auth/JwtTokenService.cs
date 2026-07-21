using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SyncBackend.Api.Entities;

namespace SyncBackend.Api.Auth;

/// <summary>
/// Builds signed JWTs for authenticated users. This is the "issue" half of auth
/// (login calls it); the "verify" half is the JwtBearer middleware in Program.cs,
/// which is configured with the *same* signing key so it can trust these tokens.
/// </summary>
public class JwtTokenService
{
    private readonly JwtSettings _settings;

    // IOptions<JwtSettings> is how DI hands us the bound configuration object.
    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Create a signed token for the given user. The returned string is what the
    /// client stores and sends back as "Authorization: Bearer &lt;token&gt;".
    /// </summary>
    public string CreateToken(User user)
    {
        // Claims = the facts we put inside the token. These are readable by anyone
        // (the payload is only base64, not encrypted), so keep them non-secret.
        var claims = new List<Claim>
        {
            // "sub" (subject) = who the token is about. We use the user's id — that's
            // what the sync endpoints will read to scope every query to this user.
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),

            // "jti" = a unique id per token. Handy later for revocation/debugging.
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // Turn the secret string into a key, then wrap it in signing credentials.
        // HMAC-SHA256 is symmetric: the same key signs here and verifies in the middleware.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenLifetimeMinutes),
            signingCredentials: credentials);

        // Serialize to the "header.payload.signature" string.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
