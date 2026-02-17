using Microsoft.EntityFrameworkCore;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<UserTermsAcceptance> UserTermsAcceptances => Set<UserTermsAcceptance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("Tenants");
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.Name).HasMaxLength(200);
            e.Property(t => t.Slug).HasMaxLength(100);
        });

        modelBuilder.Entity<UserTenant>(e =>
        {
            e.ToTable("UserTenants");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.KeycloakUserId, x.TenantId }).IsUnique();
            e.Property(x => x.KeycloakUserId).HasMaxLength(450);
            e.Property(x => x.Role).HasMaxLength(100);
            e.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserTermsAcceptance>(e =>
        {
            e.ToTable("UserTermsAcceptances");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.KeycloakUserId);
            e.Property(x => x.KeycloakUserId).HasMaxLength(450);
            e.Property(x => x.TermsVersion).HasMaxLength(50);
        });
    }
}
