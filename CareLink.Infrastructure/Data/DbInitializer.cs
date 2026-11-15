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
            foreach (var role in UserRoles.AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Seeded role: {Role}", role);
                }
            }

            // 2. Seed Default Organization
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

            // 3. Seed Default System Admin
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
                    await userManager.AddToRoleAsync(adminUser, UserRoles.SystemAdmin);
                    logger.LogInformation("Seeded default admin user: {Email}", adminEmail);
                }
            }

            // 4. Seed Default Coordinator
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

            // 5. Seed Default Citizen
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
