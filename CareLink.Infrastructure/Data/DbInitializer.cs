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
                // Ensure existing admin user has both Admin and SystemAdmin
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
                    EmailConfirmed = true,
                    IsActive = true,
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
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(citizenUser, "Citizen@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(citizenUser, UserRoles.Citizen);
                    logger.LogInformation("Seeded default citizen user: {Email}", citizenEmail);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
