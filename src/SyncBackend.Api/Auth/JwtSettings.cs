namespace SyncBackend.Api.Auth;

/// <summary>
/// Strongly-typed view of the "Jwt" configuration section. Bound once in Program.cs
/// so the rest of the app reads these values through a typed object instead of
/// stringly-keyed <c>Configuration["Jwt:..."]</c> lookups.
///
/// Split across two files on purpose:
///   - Issuer / Audience / lifetime  → appsettings.json (not secret, safe to commit)
///   - SigningKey                     → appsettings.Development.json (gitignored; env var in prod)
/// .NET merges both sections at startup.
/// </summary>
public class JwtSettings
{
    /// <summary>Who issued the token. Written into the token and checked on validation.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Who the token is intended for. Also written in and checked on validation.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>How long a freshly issued token stays valid.</summary>
    public int AccessTokenLifetimeMinutes { get; set; }

    /// <summary>
    /// The HMAC secret used to both sign (at login) and verify (on every request).
    /// Must be long + random and never committed. Empty here; supplied by config.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;
}
