using CareLink.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables for clarity
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            entity.Property(u => u.NIC).HasMaxLength(20);
            entity.Property(u => u.Address).HasMaxLength(250);

            entity.HasOne(u => u.Organization)
                  .WithMany(o => o.Users)
                  .HasForeignKey(u => u.OrganizationId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
        builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UserTokens"));

        builder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organizations");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Name).HasMaxLength(200).IsRequired();
            entity.Property(o => o.Type).HasMaxLength(100).IsRequired();
            entity.Property(o => o.ContactEmail).HasMaxLength(150);
            entity.Property(o => o.ContactPhone).HasMaxLength(50);
        });
    }
}
