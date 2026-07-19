using Microsoft.EntityFrameworkCore;
using SyncBackend.Api.Entities;

namespace SyncBackend.Api.Data;

/// <summary>
/// The EF Core database session. Each <see cref="DbSet{T}"/> maps to a table, and
/// OnModelCreating declares the schema rules (keys, indexes, relationships) that the
/// first migration will turn into real Postgres tables.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<VaultFile> VaultFiles => Set<VaultFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(user =>
        {
            // One account per email address.
            user.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<VaultFile>(file =>
        {
            // A given path is unique *within* a user's vault (two users may share a path).
            file.HasIndex(f => new { f.UserId, f.Path }).IsUnique();

            // Deleting a user removes all of their files.
            file.HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
