using CareLink.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Data;

public class CareLinkDbContext : DbContext
{
    public CareLinkDbContext(DbContextOptions<CareLinkDbContext> options) : base(options)
    {
    }

    public DbSet<AccommodationRequest> AccommodationRequests => Set<AccommodationRequest>();
    public DbSet<Shelter> Shelters => Set<Shelter>();
    public DbSet<DisasterUpdate> DisasterUpdates => Set<DisasterUpdate>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<HealthSafetyIncident> HealthSafetyIncidents => Set<HealthSafetyIncident>();
    public DbSet<SafetyCheck> SafetyChecks => Set<SafetyCheck>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<NotificationItem> Notifications => Set<NotificationItem>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<Admission> Admissions => Set<Admission>();
    public DbSet<OccupancyThreshold> OccupancyThresholds => Set<OccupancyThreshold>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<ResourceItem> ResourceItems => Set<ResourceItem>();
    public DbSet<ResourceDistribution> ResourceDistributions => Set<ResourceDistribution>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<ShiftCoverage> ShiftCoverages => Set<ShiftCoverage>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<DonationRecord> DonationRecords => Set<DonationRecord>();
    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Expenditure> Expenditures => Set<Expenditure>();
    public DbSet<FlaggedTransaction> FlaggedTransactions => Set<FlaggedTransaction>();
    public DbSet<FundAllocation> FundAllocations => Set<FundAllocation>();
    public DbSet<FundingSettings> FundingSettings => Set<FundingSettings>();
    public DbSet<ResourceContribution> ResourceContributions => Set<ResourceContribution>();
    public DbSet<TransparencyReport> TransparencyReports => Set<TransparencyReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccommodationRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Applicant).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CurrentLocation).IsRequired().HasMaxLength(250);
            entity.Property(e => e.RequestedShelter).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ShelterId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Shelter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(250);
            entity.Property(e => e.District).HasMaxLength(150);
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.Manager).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<DisasterUpdate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Source).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.PublishedBy).HasMaxLength(150);
        });

        modelBuilder.Entity<EmergencyContact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Number).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<HealthSafetyIncident>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ReportedBy).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AssignedStaff).HasMaxLength(150);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        });

        modelBuilder.Entity<SafetyCheck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.IconKey).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(50);
        });

        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Issue).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Shelter).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Room).HasMaxLength(150);
            entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Technician).HasMaxLength(150);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        });

        modelBuilder.Entity<NotificationItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Admission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Resident).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ShelterId).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<OccupancyThreshold>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<ReportDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IconKey).HasMaxLength(50);
        });

        modelBuilder.Entity<Resident>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Gender).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Contact).HasMaxLength(50);
            entity.Property(e => e.FamilyGroup).HasMaxLength(50);
            entity.Property(e => e.ShelterId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShelterName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Room).HasMaxLength(50);
            entity.Property(e => e.Bed).HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AssistanceNeeds).HasMaxLength(200);
        });

        modelBuilder.Entity<ResourceItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShelterId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShelterName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<ResourceDistribution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ResourceId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ResourceName).IsRequired().HasMaxLength(250);
            entity.Property(e => e.DestinationShelter).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DestinationShelterId).HasMaxLength(50);
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ShelterId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShelterName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Floor).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.HasMany(e => e.Beds).WithOne().HasForeignKey(b => b.RoomId);
        });

        modelBuilder.Entity<Bed>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BedNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RoomId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ResidentName).HasMaxLength(150);
        });

        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ShelterName).HasMaxLength(200);
            entity.Property(e => e.ShelterLocation).HasMaxLength(150);
            entity.Property(e => e.ShelterCapacity).HasMaxLength(20);
            entity.Property(e => e.ShelterManager).HasMaxLength(200);
            entity.Property(e => e.ShelterAddress).HasMaxLength(300);
        });

        modelBuilder.Entity<StaffMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Designation).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Contact).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Shelter).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Shift).HasMaxLength(50);
            entity.Property(e => e.Availability).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<ShiftCoverage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Day).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Shelter).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Event).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Period).HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Target).HasPrecision(18, 2);
            entity.Property(e => e.Raised).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<DonationRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Donor).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NumericAmount).HasPrecision(18, 2);
            entity.Property(e => e.Campaign).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Received).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Verification).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Allocation).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<Donor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Contact).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Scope).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Verification).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Contributions).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<Expenditure>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Invoice).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Date).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Verification).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<FlaggedTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Record).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.AmountDisplay).HasMaxLength(50);
            entity.Property(e => e.Party).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Flagged).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Investigator).HasMaxLength(200);
            entity.Property(e => e.RecordType).HasMaxLength(50);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(1000);
        });

        modelBuilder.Entity<FundAllocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Purpose).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Date).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Approval).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Disbursement).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<FundingSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Contact).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.ApprovalLimit).HasPrecision(18, 2);
            entity.Property(e => e.ActiveDevice).HasMaxLength(100);
            entity.Property(e => e.LastPasswordUpdate).HasMaxLength(100);
        });

        modelBuilder.Entity<ResourceContribution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Donor).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Quantity).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Campaign).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Received).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Verification).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Allocation).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<TransparencyReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Reference).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Date).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Period).HasMaxLength(100);
            entity.Property(e => e.VerifiedContributions).HasPrecision(18, 2);
            entity.Property(e => e.AllocatedFunds).HasPrecision(18, 2);
            entity.Property(e => e.Expenditure).HasPrecision(18, 2);
            entity.Property(e => e.AvailableBalance).HasPrecision(18, 2);
            entity.Property(e => e.Author).HasMaxLength(100);
        });
    }
}