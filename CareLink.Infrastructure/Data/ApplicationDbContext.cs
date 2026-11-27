using CareLink.Core.Constants;
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
    public DbSet<OrganizationDocument> OrganizationDocuments => Set<OrganizationDocument>();
    public DbSet<AiRecommendation> AiRecommendations => Set<AiRecommendation>();
    public DbSet<ChatbotConversation> ChatbotConversations => Set<ChatbotConversation>();
    public DbSet<ChatbotMessage> ChatbotMessages => Set<ChatbotMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AiRecommendation>(entity =>
        {
            entity.ToTable("AiRecommendations");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Category).HasMaxLength(100).IsRequired();
            entity.Property(r => r.Title).HasMaxLength(250).IsRequired();
            entity.Property(r => r.RecommendedPriority).HasMaxLength(50).IsRequired();
            entity.Property(r => r.ReviewState).HasMaxLength(50).IsRequired();
            entity.Property(r => r.ModelName).HasMaxLength(100);
            entity.Property(r => r.CrisisId).HasMaxLength(50);
            entity.Property(r => r.RequestId).HasMaxLength(50);
        });

        // Rename Identity tables for clarity
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            entity.Property(u => u.NIC).HasMaxLength(20);
            entity.Property(u => u.Address).HasMaxLength(250);
            entity.Property(u => u.Designation).HasMaxLength(150);
            entity.Property(u => u.AlternativePhoneNumber).HasMaxLength(50);
            entity.Property(u => u.AccountStatus).HasMaxLength(50).HasDefaultValue(AccountStatuses.Active);
            entity.Property(u => u.RejectionReason).HasMaxLength(500);
            entity.Property(u => u.ApprovedBy).HasMaxLength(150);
            entity.Property(u => u.Skills).HasMaxLength(500);
            entity.Property(u => u.Availability).HasMaxLength(100);
            entity.Property(u => u.PreferredLocation).HasMaxLength(150);

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
            entity.Property(o => o.Id).ValueGeneratedNever();
            entity.Property(o => o.Name).HasMaxLength(200).IsRequired();
            entity.Property(o => o.Type).HasMaxLength(100).IsRequired();
            entity.Property(o => o.OtherOrganizationType).HasMaxLength(150);
            entity.Property(o => o.RegistrationCountry).HasMaxLength(100).HasDefaultValue("Sri Lanka");
            entity.Property(o => o.RegistrationNumber).HasMaxLength(100);
            entity.Property(o => o.Website).HasMaxLength(250);
            entity.Property(o => o.ContactEmail).HasMaxLength(150);
            entity.Property(o => o.ContactPhone).HasMaxLength(50);
            entity.Property(o => o.SecondaryContactPhone).HasMaxLength(50);
            entity.Property(o => o.Address).HasMaxLength(350);
            entity.Property(o => o.District).HasMaxLength(100);
            entity.Property(o => o.Province).HasMaxLength(100);
            entity.Property(o => o.Description).HasMaxLength(2000);
            entity.Property(o => o.AreasOfOperation).HasMaxLength(1000);
            entity.Property(o => o.AreasOfAssistance).HasMaxLength(1000);
            entity.Property(o => o.OtherAreaOfAssistance).HasMaxLength(250);
            entity.Property(o => o.OperationalCapacity).HasMaxLength(2000);
            entity.Property(o => o.EmergencyResponseAvailability).HasMaxLength(100);
            entity.Property(o => o.AdditionalResources).HasMaxLength(2000);
            entity.Property(o => o.RepresentativeUserId).HasMaxLength(450);
            entity.Property(o => o.RepresentativeFirstName).HasMaxLength(100);
            entity.Property(o => o.RepresentativeLastName).HasMaxLength(100);
            entity.Property(o => o.RepresentativeFullName).HasMaxLength(200);
            entity.Property(o => o.RepresentativeDesignation).HasMaxLength(150);
            entity.Property(o => o.RepresentativeEmail).HasMaxLength(150);
            entity.Property(o => o.RepresentativePhone).HasMaxLength(50);
            entity.Property(o => o.RepresentativeAlternativePhone).HasMaxLength(50);
            entity.Property(o => o.ApprovalStatus).HasMaxLength(50).HasDefaultValue(AccountStatuses.Active);
            entity.Property(o => o.RejectionReason).HasMaxLength(1000);
            entity.Property(o => o.AdditionalInfoRequest).HasMaxLength(1000);
            entity.Property(o => o.AdditionalInfoResponse).HasMaxLength(2000);
            entity.Property(o => o.ReviewedBy).HasMaxLength(150);

            entity.HasMany(o => o.Documents)
                  .WithOne(d => d.Organization)
                  .HasForeignKey(d => d.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrganizationDocument>(entity =>
        {
            entity.ToTable("OrganizationDocuments");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).ValueGeneratedNever();
            entity.Property(d => d.DocumentCategory).HasMaxLength(100).IsRequired();
            entity.Property(d => d.DocumentLabel).HasMaxLength(200).IsRequired();
            entity.Property(d => d.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(d => d.StoredFileName).HasMaxLength(260).IsRequired();
            entity.Property(d => d.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(d => d.FileSizeDisplay).HasMaxLength(50);
            entity.Property(d => d.StoragePath).HasMaxLength(500);
            entity.Property(d => d.UploadedBy).HasMaxLength(150);
        });

        builder.Entity<ChatbotConversation>(entity =>
        {
            entity.ToTable("ChatbotConversations");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.UserId).HasMaxLength(450).IsRequired();
            entity.Property(c => c.Title).HasMaxLength(250).IsRequired();
            entity.Property(c => c.UserRole).HasMaxLength(50).IsRequired();
            entity.HasIndex(c => c.UserId);

            entity.HasMany(c => c.Messages)
                  .WithOne(m => m.Conversation)
                  .HasForeignKey(m => m.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChatbotMessage>(entity =>
        {
            entity.ToTable("ChatbotMessages");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.ConversationId).HasMaxLength(450).IsRequired();
            entity.Property(m => m.Role).HasMaxLength(50).IsRequired();
            entity.Property(m => m.Content).IsRequired();
            entity.HasIndex(m => m.ConversationId);
        });
    }
}
