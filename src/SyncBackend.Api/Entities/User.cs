namespace SyncBackend.Api.Entities;

/// <summary>
/// A registered account. Each user owns an isolated set of <see cref="VaultFile"/>
/// records — one person's vault must never be visible to another. Auth (password
/// hashing / JWT) is added in Phase 1; for now this is just the identity a file
/// belongs to.
/// </summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>Login identifier. Unique across all users (enforced in the DbContext).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password (never the plain text). Populated in Phase 1 when auth lands.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Navigation property: all files this user owns.</summary>
    public List<VaultFile> Files { get; set; } = new();
}
