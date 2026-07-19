namespace SyncBackend.Api.Entities;

/// <summary>
/// The server-side copy of one markdown file in a user's vault. This is the core
/// sync record: clients push local changes up and pull remote changes down against
/// these rows. Sync is file-level (whole files), so there are no per-task IDs here.
/// </summary>
public class VaultFile
{
    public Guid Id { get; set; }

    /// <summary>Owner. Files are always scoped to a single user.</summary>
    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Vault-relative path, e.g. "Daily/2026-07-19.md". Unique per user — the
    /// (UserId, Path) pair identifies a file (enforced in the DbContext).
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Raw file contents. Markdown files are tiny, so stored directly in Postgres.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Content fingerprint for cheap change detection (client compares hashes).</summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Per-user monotonically increasing counter. Drives incremental pulls
    /// ("give me everything changed since revision N") and optimistic concurrency.
    /// </summary>
    public long Revision { get; set; }

    /// <summary>Soft-delete tombstone. A deleted file stays as a row so other devices learn it was removed.</summary>
    public bool IsDeleted { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
