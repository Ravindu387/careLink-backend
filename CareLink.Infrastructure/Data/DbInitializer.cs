using System.Security.Claims;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CareLink.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        try
        {
            // Apply pending migrations or ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Non-destructive schema evolution for existing SQL Server databases created via EnsureCreated
            if (context.Database.IsSqlServer())
            {
                await EnsureSqlServerSchemaColumnsAsync(context, logger);
            }

            // 1. Seed Roles
            foreach (var roleName in UserRoles.AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation("Seeded role: {Role}", roleName);
                }
            }

            // 2. Seed Default Role Permissions (RoleClaims)
            foreach (var roleName in UserRoles.AllRoles)
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var existingClaims = await roleManager.GetClaimsAsync(role);
                var existingPermClaims = existingClaims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var defaults = DefaultRolePermissions.GetDefaultPermissions(roleName);
                foreach (var perm in defaults)
                {
                    if (!existingPermClaims.Contains(perm))
                    {
                        await roleManager.AddClaimAsync(role, new Claim("Permission", perm));
                    }
                }
            }
            logger.LogInformation("Default role-permission matrices seeded successfully.");

            // 3. Seed Default Organization
            var org = await context.Organizations.FirstOrDefaultAsync(o => o.Name == "National Disaster Management Centre");
            if (org == null)
            {
                org = new Organization
                {
                    Name = "National Disaster Management Centre",
                    Type = "Government",
                    RegistrationNumber = "GOV-DMC-001",
                    ContactEmail = "info@dmc.gov.lk",
                    ContactPhone = "+94112000000",
                    IsActive = true,
                    ApprovalStatus = AccountStatuses.Active,
                    CreatedAt = DateTime.UtcNow
                };
                context.Organizations.Add(org);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default organization: {OrgName}", org.Name);
            }

            // 4. Seed Default Admin User
            const string adminEmail = "admin@carelink.org";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    PhoneNumber = "+94770000000",
                    NIC = "199000000000",
                    OrganizationId = org.Id,
                    EmailConfirmed = true,
                    IsActive = true,
                    AccountStatus = AccountStatuses.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                    await userManager.AddToRoleAsync(adminUser, UserRoles.SystemAdmin);
                    logger.LogInformation("Seeded default admin user: {Email}", adminEmail);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, UserRoles.Admin))
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                if (!await userManager.IsInRoleAsync(adminUser, UserRoles.SystemAdmin))
                    await userManager.AddToRoleAsync(adminUser, UserRoles.SystemAdmin);
            }

            // 5. Seed Default Coordinator
            const string coordEmail = "coordinator@carelink.org";
            var coordUser = await userManager.FindByEmailAsync(coordEmail);
            if (coordUser == null)
            {
                coordUser = new ApplicationUser
                {
                    UserName = coordEmail,
                    Email = coordEmail,
                    FullName = "Emergency Operations Coordinator",
                    PhoneNumber = "+94771112222",
                    NIC = "199200000000",
                    OrganizationId = org.Id,
                    EmailConfirmed = true,
                    IsActive = true,
                    AccountStatus = AccountStatuses.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(coordUser, "Coordinator@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordUser, UserRoles.EmergencyCoordinator);
                    logger.LogInformation("Seeded default coordinator user: {Email}", coordEmail);
                }
            }

            // 6. Seed Default Partner Officer
            const string partnerEmail = "partner@carelink.org";
            var partnerUser = await userManager.FindByEmailAsync(partnerEmail);
            if (partnerUser == null)
            {
                partnerUser = new ApplicationUser
                {
                    UserName = partnerEmail,
                    Email = partnerEmail,
                    FullName = "Partner Operations Officer",
                    PhoneNumber = "+94772223333",
                    NIC = "199300000000",
                    OrganizationId = org.Id,
                    EmailConfirmed = true,
                    IsActive = true,
                    AccountStatus = AccountStatuses.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(partnerUser, "Partner@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(partnerUser, UserRoles.PartnerOfficer);
                    logger.LogInformation("Seeded default partner officer user: {Email}", partnerEmail);
                }
            }

            // 7. Seed Default Volunteer
            const string volunteerEmail = "volunteer@carelink.org";
            var volunteerUser = await userManager.FindByEmailAsync(volunteerEmail);
            if (volunteerUser == null)
            {
                volunteerUser = new ApplicationUser
                {
                    UserName = volunteerEmail,
                    Email = volunteerEmail,
                    FullName = "Field Response Volunteer",
                    PhoneNumber = "+94775556666",
                    NIC = "199400000000",
                    OrganizationId = org.Id,
                    Skills = "First Aid, Search & Rescue",
                    Availability = "Immediate",
                    PreferredLocation = "Greater Riverton",
                    EmailConfirmed = true,
                    IsActive = true,
                    AccountStatus = AccountStatuses.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(volunteerUser, "Volunteer@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(volunteerUser, UserRoles.Volunteer);
                    logger.LogInformation("Seeded default volunteer user: {Email}", volunteerEmail);
                }
            }

            // 8. Seed Default Citizen
            const string citizenEmail = "citizen@carelink.org";
            var citizenUser = await userManager.FindByEmailAsync(citizenEmail);
            if (citizenUser == null)
            {
                citizenUser = new ApplicationUser
                {
                    UserName = citizenEmail,
                    Email = citizenEmail,
                    FullName = "Citizen User",
                    PhoneNumber = "+94773334444",
                    NIC = "199500000000",
                    Address = "123 Galle Road, Colombo 03",
                    EmailConfirmed = true,
                    IsActive = true,
                    AccountStatus = AccountStatuses.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(citizenUser, "Citizen@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(citizenUser, UserRoles.Citizen);
                    logger.LogInformation("Seeded default citizen user: {Email}", citizenEmail);
                }
            }

            // 9. Seed Default Accounts for Remaining Official Roles (CaseManager, FundingOfficer, ResourceManager, ShelterAdministrator, VolunteerCoordinator)
            await SeedRoleAccountIfMissingAsync(userManager, org.Id, "casemanager@carelink.org", "Amina Mensah", "CaseManager@123456", UserRoles.CaseManager, logger);
            await SeedRoleAccountIfMissingAsync(userManager, org.Id, "funding@carelink.org", "Maria Kone", "Funding@123456", UserRoles.FundingOfficer, logger);
            await SeedRoleAccountIfMissingAsync(userManager, org.Id, "resourcemanager@carelink.org", "Daniel Perera", "Resource@123456", UserRoles.ResourceManager, logger);
            await SeedRoleAccountIfMissingAsync(userManager, org.Id, "shelteradmin@carelink.org", "Sanjana Amarasinghe", "Shelter@123456", UserRoles.ShelterAdministrator, logger);
            await SeedRoleAccountIfMissingAsync(userManager, org.Id, "volunteercoordinator@carelink.org", "Nadia Okafor", "VolunteerCoord@123456", UserRoles.VolunteerCoordinator, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedRoleAccountIfMissingAsync(
        UserManager<ApplicationUser> userManager,
        Guid organizationId,
        string email,
        string fullName,
        string password,
        string roleName,
        ILogger logger)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            if (!await userManager.IsInRoleAsync(existing, roleName))
            {
                await userManager.AddToRoleAsync(existing, roleName);
            }

            var needsUpdate = false;
            if (!existing.IsActive ||
                existing.RequiresPasswordSetup ||
                !existing.EmailConfirmed ||
                !string.Equals(existing.AccountStatus, AccountStatuses.Active, StringComparison.OrdinalIgnoreCase))
            {
                existing.IsActive = true;
                existing.RequiresPasswordSetup = false;
                existing.PasswordSetupTokenUsed = true;
                existing.EmailConfirmed = true;
                existing.AccountStatus = AccountStatuses.Active;
                needsUpdate = true;
            }

            if (!await userManager.CheckPasswordAsync(existing, password))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(existing);
                await userManager.ResetPasswordAsync(existing, resetToken, password);
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await userManager.UpdateAsync(existing);
                logger.LogInformation("Restored active state and credentials for seeded {Role} user: {Email}", roleName, email);
            }

            return;
        }

        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            PhoneNumber = "+94770001122",
            OrganizationId = organizationId,
            EmailConfirmed = true,
            IsActive = true,
            AccountStatus = AccountStatuses.Active,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(newUser, password);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(newUser, roleName);
            logger.LogInformation("Seeded default {Role} user: {Email}", roleName, email);
        }
    }

    private static async Task EnsureSqlServerSchemaColumnsAsync(ApplicationDbContext context, ILogger logger)
    {
        const string sql = @"
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Users', 'AccountStatus') IS NULL
        ALTER TABLE [dbo].[Users] ADD [AccountStatus] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Users_AccountStatus] DEFAULT N'Active';

    IF COL_LENGTH('dbo.Users', 'RejectionReason') IS NULL
        ALTER TABLE [dbo].[Users] ADD [RejectionReason] NVARCHAR(500) NULL;

    IF COL_LENGTH('dbo.Users', 'ApprovedAt') IS NULL
        ALTER TABLE [dbo].[Users] ADD [ApprovedAt] DATETIME2 NULL;

    IF COL_LENGTH('dbo.Users', 'ApprovedBy') IS NULL
        ALTER TABLE [dbo].[Users] ADD [ApprovedBy] NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.Users', 'Skills') IS NULL
        ALTER TABLE [dbo].[Users] ADD [Skills] NVARCHAR(500) NULL;

    IF COL_LENGTH('dbo.Users', 'Availability') IS NULL
        ALTER TABLE [dbo].[Users] ADD [Availability] NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.Users', 'PreferredLocation') IS NULL
        ALTER TABLE [dbo].[Users] ADD [PreferredLocation] NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.Users', 'RequiresPasswordSetup') IS NULL
        ALTER TABLE [dbo].[Users] ADD [RequiresPasswordSetup] BIT NOT NULL CONSTRAINT [DF_Users_RequiresPasswordSetup] DEFAULT 0;

    IF COL_LENGTH('dbo.Users', 'PasswordSetupTokenUsed') IS NULL
        ALTER TABLE [dbo].[Users] ADD [PasswordSetupTokenUsed] BIT NOT NULL CONSTRAINT [DF_Users_PasswordSetupTokenUsed] DEFAULT 0;

    IF COL_LENGTH('dbo.Users', 'PasswordSetupTokenExpiresAt') IS NULL
        ALTER TABLE [dbo].[Users] ADD [PasswordSetupTokenExpiresAt] DATETIME2 NULL;

    IF COL_LENGTH('dbo.Users', 'Designation') IS NULL
        ALTER TABLE [dbo].[Users] ADD [Designation] NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.Users', 'AlternativePhoneNumber') IS NULL
        ALTER TABLE [dbo].[Users] ADD [AlternativePhoneNumber] NVARCHAR(50) NULL;
END;

IF OBJECT_ID(N'dbo.Organizations', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Organizations', 'Description') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [Description] NVARCHAR(2000) NULL;

    IF COL_LENGTH('dbo.Organizations', 'Address') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [Address] NVARCHAR(350) NULL;

    IF COL_LENGTH('dbo.Organizations', 'ApprovalStatus') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [ApprovalStatus] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Organizations_ApprovalStatus] DEFAULT N'Active';

    IF COL_LENGTH('dbo.Organizations', 'RejectionReason') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RejectionReason] NVARCHAR(1000) NULL;

    IF COL_LENGTH('dbo.Organizations', 'OtherOrganizationType') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [OtherOrganizationType] NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.Organizations', 'RegistrationCountry') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RegistrationCountry] NVARCHAR(100) NOT NULL CONSTRAINT [DF_Organizations_RegistrationCountry] DEFAULT N'Sri Lanka';

    IF COL_LENGTH('dbo.Organizations', 'YearEstablished') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [YearEstablished] INT NULL;

    IF COL_LENGTH('dbo.Organizations', 'Website') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [Website] NVARCHAR(250) NULL;

    IF COL_LENGTH('dbo.Organizations', 'SecondaryContactPhone') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [SecondaryContactPhone] NVARCHAR(50) NULL;

    IF COL_LENGTH('dbo.Organizations', 'District') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [District] NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.Organizations', 'Province') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [Province] NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.Organizations', 'AreasOfOperation') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [AreasOfOperation] NVARCHAR(1000) NULL;

    IF COL_LENGTH('dbo.Organizations', 'AreasOfAssistance') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [AreasOfAssistance] NVARCHAR(1000) NULL;

    IF COL_LENGTH('dbo.Organizations', 'OtherAreaOfAssistance') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [OtherAreaOfAssistance] NVARCHAR(250) NULL;

    IF COL_LENGTH('dbo.Organizations', 'OperationalCapacity') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [OperationalCapacity] NVARCHAR(2000) NULL;

    IF COL_LENGTH('dbo.Organizations', 'EmergencyResponseAvailability') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [EmergencyResponseAvailability] NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.Organizations', 'AdditionalResources') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [AdditionalResources] NVARCHAR(2000) NULL;

    IF COL_LENGTH('dbo.Organizations', 'RepresentativeUserId') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RepresentativeUserId] NVARCHAR(450) NULL;

    IF COL_LENGTH('dbo.Organizations', 'RepresentativeFirstName') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RepresentativeFirstName] NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.Organizations', 'RepresentativeLastName') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RepresentativeLastName] NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.Organizations', 'RepresentativeFullName') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RepresentativeFullName] NVARCHAR(200) NULL;

    IF COL_LENGTH('dbo.Organizations', 'RepresentativeDesignation') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RepresentativeDesignation] NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.Organizations', 'RepresentativeEmail') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RepresentativeEmail] NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.Organizations', 'RepresentativePhone') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RepresentativePhone] NVARCHAR(50) NULL;

    IF COL_LENGTH('dbo.Organizations', 'RepresentativeAlternativePhone') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [RepresentativeAlternativePhone] NVARCHAR(50) NULL;

    IF COL_LENGTH('dbo.Organizations', 'ConfirmedAuthorisedRepresentative') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [ConfirmedAuthorisedRepresentative] BIT NOT NULL CONSTRAINT [DF_Organizations_ConfirmedAuthRep] DEFAULT 0;

    IF COL_LENGTH('dbo.Organizations', 'ConfirmedAccurateInformation') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [ConfirmedAccurateInformation] BIT NOT NULL CONSTRAINT [DF_Organizations_ConfirmedAccurate] DEFAULT 0;

    IF COL_LENGTH('dbo.Organizations', 'AgreedToTermsAndPrivacy') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [AgreedToTermsAndPrivacy] BIT NOT NULL CONSTRAINT [DF_Organizations_AgreedTerms] DEFAULT 0;

    IF COL_LENGTH('dbo.Organizations', 'AdditionalInfoRequest') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [AdditionalInfoRequest] NVARCHAR(1000) NULL;

    IF COL_LENGTH('dbo.Organizations', 'AdditionalInfoResponse') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [AdditionalInfoResponse] NVARCHAR(2000) NULL;

    IF COL_LENGTH('dbo.Organizations', 'AdditionalInfoRequestedAt') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [AdditionalInfoRequestedAt] DATETIME2 NULL;

    IF COL_LENGTH('dbo.Organizations', 'AdditionalInfoSubmittedAt') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [AdditionalInfoSubmittedAt] DATETIME2 NULL;

    IF COL_LENGTH('dbo.Organizations', 'ReviewedBy') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [ReviewedBy] NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.Organizations', 'ReviewedAt') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [ReviewedAt] DATETIME2 NULL;

    IF COL_LENGTH('dbo.Organizations', 'UpdatedAt') IS NULL
        ALTER TABLE [dbo].[Organizations] ADD [UpdatedAt] DATETIME2 NULL;
END;

IF OBJECT_ID(N'dbo.OrganizationDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrganizationDocuments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
        [DocumentCategory] NVARCHAR(100) NOT NULL,
        [DocumentLabel] NVARCHAR(200) NOT NULL,
        [OriginalFileName] NVARCHAR(260) NOT NULL,
        [StoredFileName] NVARCHAR(260) NOT NULL,
        [ContentType] NVARCHAR(100) NOT NULL,
        [FileSizeBytes] BIGINT NOT NULL,
        [FileSizeDisplay] NVARCHAR(50) NULL,
        [StoragePath] NVARCHAR(500) NULL,
        [FileContent] VARBINARY(MAX) NULL,
        [UploadedBy] NVARCHAR(150) NULL,
        [UploadedAt] DATETIME2 NOT NULL,
        CONSTRAINT [FK_OrganizationDocuments_Organizations_OrganizationId]
            FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'dbo.ChatbotConversations', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChatbotConversations] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Title] NVARCHAR(250) NOT NULL,
        [UserRole] NVARCHAR(50) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL
    );
    CREATE INDEX [IX_ChatbotConversations_UserId] ON [dbo].[ChatbotConversations] ([UserId]);
END;

IF OBJECT_ID(N'dbo.ChatbotMessages', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChatbotMessages] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [ConversationId] NVARCHAR(450) NOT NULL,
        [Role] NVARCHAR(50) NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [Timestamp] DATETIME2 NOT NULL,
        CONSTRAINT [FK_ChatbotMessages_Conversations] FOREIGN KEY ([ConversationId]) 
            REFERENCES [dbo].[ChatbotConversations] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_ChatbotMessages_ConversationId] ON [dbo].[ChatbotMessages] ([ConversationId]);
END;";

        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL AND COL_LENGTH('dbo.Users', 'AccountStatus') IS NOT NULL
BEGIN
    EXEC(N'UPDATE [dbo].[Users] SET [AccountStatus] = CASE WHEN [IsActive] = 1 THEN N''Active'' ELSE N''Disabled'' END WHERE [AccountStatus] IS NULL OR [AccountStatus] = N''''');
END;");
            logger.LogInformation("Verified non-destructive SQL Server schema columns for Users and Organizations.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Non-fatal warning while checking SQL Server column migrations.");
        }
    }
}
