using System.Security.Claims;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CareLink.API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        if (userManager != null)
        {
            var dbUser = await userManager.FindByIdAsync(userId);
            if (dbUser != null)
            {
                // Enforce account status at the API authorization level:
                // PendingApproval, Rejected, Disabled, or setup-incomplete accounts cannot access protected resources.
                if (!dbUser.IsActive ||
                    dbUser.RequiresPasswordSetup ||
                    !string.Equals(dbUser.AccountStatus, AccountStatuses.Active, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
        }

        // Administrators automatically satisfy all permission requirements once verified active
        if (context.User.IsInRole(UserRoles.Admin) || context.User.IsInRole(UserRoles.SystemAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var hasPerm = await permissionService.HasPermissionAsync(userId, requirement.Permission);
        if (hasPerm)
        {
            context.Succeed(requirement);
        }
    }
}
