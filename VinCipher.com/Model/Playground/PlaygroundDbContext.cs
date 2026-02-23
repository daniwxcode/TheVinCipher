using Microsoft.EntityFrameworkCore;

using VinCipher.Model.Playground;

namespace VinCipher.Model;

public class PlaygroundDbContext : DbContext
{
    public PlaygroundDbContext(DbContextOptions<PlaygroundDbContext> options) : base(options) { }

    public DbSet<PlaygroundAccount> Accounts => Set<PlaygroundAccount>();
    public DbSet<PlaygroundApiToken> ApiTokens => Set<PlaygroundApiToken>();
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlaygroundAccount>(e =>
        {
            e.HasIndex(a => a.Email).IsUnique();
            e.HasMany(a => a.Tokens)
             .WithOne(t => t.Account)
             .HasForeignKey(t => t.AccountId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlaygroundApiToken>(e =>
        {
            e.HasIndex(t => t.Key).IsUnique();
            e.HasMany(t => t.RequestLogs)
             .WithOne(r => r.Token)
             .HasForeignKey(r => r.TokenId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequestLog>(e =>
        {
            e.HasIndex(r => r.TimestampUtc);
            e.HasIndex(r => r.TokenId);
        });

        modelBuilder.Entity<AdminUser>(e =>
        {
            e.HasIndex(a => a.Username).IsUnique();
        });

        modelBuilder.Entity<AccessRequest>(e =>
        {
            e.HasIndex(r => r.Email);
            e.HasIndex(r => r.Status);
        });
    }
}
