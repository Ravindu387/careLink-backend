using CareLink.Application.Common;
using CareLink.Application.DTOs.Roles;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareLink.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IPermissionService _permissionService;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<UserService> _logger;

    private static readonly HashSet<string> PrivilegedRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        UserRoles.Admin,
        UserRoles.SystemAdmin,
        UserRoles.EmergencyCoordinator,
        UserRoles.CaseManager,
        UserRoles.IncidentManager,
        UserRoles.ResourceManager,
        UserRoles.LogisticsManager,
        UserRoles.ShelterAdministrator,
        UserRoles.ShelterAdmin,
        UserRoles.VolunteerCoordinator,
        UserRoles.PartnerOfficer,
        UserRoles.FundingOfficer,
        UserRoles.Donor
    };

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IPermissionService permissionService,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _permissionService = permissionService;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedListDto<UserDetailsDto>>> GetUsersAsync(UserListQueryDto query)
    {
        var usersQuery = _userManager.Users
            .Include(u => u.Organization)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            usersQuery = usersQuery.Where(u =>
                u.FullName.ToLower().Contains(search) ||
                (u.Email != null && u.Email.ToLower().Contains(search)) ||
                (u.NIC != null && u.NIC.ToLower().Contains(search)) ||
                (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(search)) ||
                (u.Organization != null && u.Organization.Name.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            var statusFilter = query.Status.Trim();
            if (statusFilter.Equals(AccountStatuses.Active, StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => u.IsActive && u.AccountStatus == AccountStatuses.Active);
            }
            else if (statusFilter.Equals(AccountStatuses.PendingPasswordSetup, StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => u.AccountStatus == AccountStatuses.PendingPasswordSetup || u.RequiresPasswordSetup);
            }
            else if (statusFilter.Equals(AccountStatuses.Disabled, StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => !u.IsActive && u.AccountStatus == AccountStatuses.Disabled);
            }
            else if (statusFilter.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                     statusFilter.Equals(AccountStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase) ||
                     statusFilter.Equals("Pending Approval", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => u.AccountStatus == AccountStatuses.PendingApproval);
            }
            else if (statusFilter.Equals(AccountStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => u.AccountStatus == AccountStatuses.Rejected);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Organization) && !query.Organization.Equals("All organisations", StringComparison.OrdinalIgnoreCase))
        {
            usersQuery = usersQuery.Where(u => u.Organization != null && u.Organization.Name == query.Organization);
        }

        if (!string.IsNullOrWhiteSpace(query.Role) && !query.Role.Equals("All roles", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedRole = NormalizeRoleName(query.Role);
            var usersInRole = await _userManager.GetUsersInRoleAsync(normalizedRole);
            var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();
            usersQuery = usersQuery.Where(u => userIdsInRole.Contains(u.Id));
        }

        if (!string.IsNullOrWhiteSpace(query.Tab) && !query.Tab.Equals("All users", StringComparison.OrdinalIgnoreCase))
        {
            if (query.Tab.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => u.AccountStatus == AccountStatuses.Disabled || (!u.IsActive && u.AccountStatus != AccountStatuses.PendingApproval && u.AccountStatus != AccountStatuses.Rejected && u.AccountStatus != AccountStatuses.PendingPasswordSetup));
            }
            else if (query.Tab.Equals("Pending approval", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => u.AccountStatus == AccountStatuses.PendingApproval);
            }
            else if (query.Tab.Equals("Pending setup", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => u.AccountStatus == AccountStatuses.PendingPasswordSetup || u.RequiresPasswordSetup);
            }
            else if (query.Tab.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => u.AccountStatus == AccountStatuses.Rejected);
            }
            else if (query.Tab.Equals("Privileged users", StringComparison.OrdinalIgnoreCase))
            {
                var privilegedUserIds = new HashSet<string>();
                foreach (var role in PrivilegedRoleNames)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    foreach (var u in usersInRole)
                    {
                        privilegedUserIds.Add(u.Id);
                    }
                }

                usersQuery = usersQuery.Where(u => privilegedUserIds.Contains(u.Id));
            }
        }

        var totalCount = await usersQuery.CountAsync();

        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 50;

        var pagedUsers = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<UserDetailsDto>();
        foreach (var user in pagedUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(MapToUserDetailsDto(user, roles));
        }

        var result = new PaginatedListDto<UserDetailsDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PaginatedListDto<UserDetailsDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<UserDetailsDto>> GetUserByIdAsync(string id)
    {
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return ApiResponse<UserDetailsDto>.FailureResult("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        var dto = MapToUserDetailsDto(user, roles, permissions);

        return ApiResponse<UserDetailsDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<UserStatsDto>> GetUserStatsAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive && u.AccountStatus == AccountStatuses.Active && !u.RequiresPasswordSetup);
        var pendingUsers = await _userManager.Users.CountAsync(u => u.AccountStatus == AccountStatuses.PendingApproval);
        var pendingSetupUsers = await _userManager.Users.CountAsync(u => u.AccountStatus == AccountStatuses.PendingPasswordSetup || u.RequiresPasswordSetup);
        var rejectedUsers = await _userManager.Users.CountAsync(u => u.AccountStatus == AccountStatuses.Rejected);
        var disabledUsers = await _userManager.Users.CountAsync(u => u.AccountStatus == AccountStatuses.Disabled);

        var privilegedUserIds = new HashSet<string>();
        foreach (var role in PrivilegedRoleNames)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            foreach (var u in usersInRole)
            {
                privilegedUserIds.Add(u.Id);
            }
        }

        var stats = new UserStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            PendingUsers = pendingUsers,
            PendingSetupUsers = pendingSetupUsers,
            RejectedUsers = rejectedUsers,
            DisabledUsers = disabledUsers,
            PrivilegedUsers = privilegedUserIds.Count
        };

        return ApiResponse<UserStatsDto>.SuccessResult(stats);
    }

    public async Task<ApiResponse<UserDetailsDto>> CreateUserAsync(CreateUserRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return ApiResponse<UserDetailsDto>.FailureResult("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiResponse<UserDetailsDto>.FailureResult("Email address is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            return ApiResponse<UserDetailsDto>.FailureResult("Role is required.");
        }

        var email = request.Email.Trim();

        var existingByEmail = await _userManager.FindByEmailAsync(email);
        var existingByName = existingByEmail ?? await _userManager.FindByNameAsync(email);
        if (existingByName != null)
        {
            return ApiResponse<UserDetailsDto>.FailureResult("An account with this email address already exists.");
        }

        if (!UserRoles.TryNormalizeRoleName(request.Role, out var assignedRole))
        {
            return ApiResponse<UserDetailsDto>.FailureResult(
                $"Invalid role: '{request.Role}'. Valid roles: {string.Join(", ", UserRoles.AllRoles)}");
        }

        Organization? organization = null;
        if (!request.OrganizationId.HasValue && !string.IsNullOrWhiteSpace(request.OrganizationName))
        {
            organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = request.OrganizationName.Trim(),
                Type = assignedRole == UserRoles.PartnerOfficer ? "Partner NGO" : "Operational Unit",
                ContactEmail = email,
                ContactPhone = request.PhoneNumber?.Trim(),
                IsActive = true,
                ApprovalStatus = AccountStatuses.Active,
                CreatedAt = DateTime.UtcNow
            };
        }

        var hasExplicitPassword = !string.IsNullOrWhiteSpace(request.Password);
        var expiryHours = _emailSettings.SetupTokenExpiryHours > 0 ? _emailSettings.SetupTokenExpiryHours : 24;
        var accountStatus = !request.IsActive
            ? AccountStatuses.Disabled
            : (hasExplicitPassword ? AccountStatuses.Active : AccountStatuses.PendingPasswordSetup);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            NIC = request.NIC?.Trim(),
            Address = request.Address?.Trim(),
            OrganizationId = organization?.Id ?? request.OrganizationId,
            Organization = organization,
            Skills = request.Skills?.Trim(),
            Availability = request.Availability?.Trim(),
            PreferredLocation = request.PreferredLocation?.Trim(),
            IsActive = request.IsActive,
            AccountStatus = accountStatus,
            EmailConfirmed = true,
            RequiresPasswordSetup = !hasExplicitPassword,
            PasswordSetupTokenUsed = false,
            PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddHours(expiryHours),
            CreatedAt = DateTime.UtcNow
        };

        var initialPassword = hasExplicitPassword
            ? request.Password!
            : $"CL!{Guid.NewGuid():N}aA1";

        var createResult = await _userManager.CreateAsync(user, initialPassword);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDetailsDto>.FailureResult("Failed to create user.", errors);
        }

        if (!await _roleManager.RoleExistsAsync(assignedRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(assignedRole));
        }

        await _userManager.AddToRoleAsync(user, assignedRole);

        // Generate secure time-limited, single-use password setup link (never returned to Admin API responses or logged)
        var rawSetupToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedSetupToken = UrlTokenEncoder.Encode(rawSetupToken);
        var setupBaseUrl = !string.IsNullOrWhiteSpace(_emailSettings.PasswordSetupBaseUrl)
            ? _emailSettings.PasswordSetupBaseUrl
            : "http://localhost:5173/setup-account";
        var setupUrl = $"{setupBaseUrl}?email={Uri.EscapeDataString(user.Email)}&token={encodedSetupToken}";
        var displayRole = FormatRoleDisplayName(assignedRole);

        var emailSent = false;
        string? failureCategory = null;
        string? failureDetail = null;
        try
        {
            var deliveryResult = await _emailService.SendAdminCreatedUserEmailWithResultAsync(
                user.Email,
                user.FullName,
                displayRole,
                setupUrl);

            if (deliveryResult != null)
            {
                emailSent = deliveryResult.Success;
                if (!deliveryResult.Success)
                {
                    failureCategory = deliveryResult.FailureCategory;
                    failureDetail = deliveryResult.DiagnosticMessage;
                }
            }
            else
            {
                emailSent = await _emailService.SendAdminCreatedUserEmailAsync(
                    user.Email,
                    user.FullName,
                    displayRole,
                    setupUrl);
                if (!emailSent)
                {
                    failureCategory = EmailFailureCategories.UnexpectedError;
                    failureDetail = "SMTP server did not accept the message.";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while dispatching invitation email to '{Email}': {ErrorType}", user.Email, ex.GetType().Name);
            emailSent = false;
            failureCategory = EmailFailureCategories.UnexpectedError;
            failureDetail = $"Email delivery threw {ex.GetType().Name}.";
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        var dto = MapToUserDetailsDto(user, roles, permissions);
        dto.EmailSent = emailSent;
        dto.EmailDeliveryStatus = emailSent ? "Sent" : "Failed";
        dto.EmailFailureCategory = failureCategory;
        dto.EmailFailureDetail = failureDetail;

        if (!emailSent)
        {
            var reasonSuffix = !string.IsNullOrWhiteSpace(failureDetail)
                ? $" Reason [{failureCategory}]: {failureDetail}"
                : " Please verify Gmail SMTP App Password configuration or use 'Resend Invitation'.";
            return ApiResponse<UserDetailsDto>.SuccessResult(
                dto,
                $"Account created for {user.FullName}, but the invitation email could not be sent to {user.Email}.{reasonSuffix}");
        }

        return ApiResponse<UserDetailsDto>.SuccessResult(
            dto,
            $"Account created successfully for {user.FullName}. An account setup email has been sent to {user.Email}.");
    }

    public async Task<ApiResponse<UserDetailsDto>> ResendInvitationAsync(string id)
    {
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return ApiResponse<UserDetailsDto>.FailureResult("User not found.");
        }

        if (string.Equals(user.AccountStatus, AccountStatuses.Disabled, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(user.AccountStatus, AccountStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<UserDetailsDto>.FailureResult("Cannot send a setup invitation to a disabled or rejected account.");
        }

        // Rotate security stamp to invalidate any previously issued setup tokens
        await _userManager.UpdateSecurityStampAsync(user);

        var expiryHours = _emailSettings.SetupTokenExpiryHours > 0 ? _emailSettings.SetupTokenExpiryHours : 24;
        user.RequiresPasswordSetup = true;
        user.PasswordSetupTokenUsed = false;
        user.PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddHours(expiryHours);
        if (!string.Equals(user.AccountStatus, AccountStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase))
        {
            user.AccountStatus = AccountStatuses.PendingPasswordSetup;
        }
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? UserRoles.Citizen;
        var displayRole = FormatRoleDisplayName(primaryRole);

        var rawSetupToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedSetupToken = UrlTokenEncoder.Encode(rawSetupToken);
        var setupBaseUrl = !string.IsNullOrWhiteSpace(_emailSettings.PasswordSetupBaseUrl)
            ? _emailSettings.PasswordSetupBaseUrl
            : "http://localhost:5173/setup-account";
        var setupUrl = $"{setupBaseUrl}?email={Uri.EscapeDataString(user.Email!)}&token={encodedSetupToken}";

        var emailSent = false;
        string? failureCategory = null;
        string? failureDetail = null;
        try
        {
            var deliveryResult = await _emailService.SendAdminCreatedUserEmailWithResultAsync(
                user.Email!,
                user.FullName,
                displayRole,
                setupUrl);

            if (deliveryResult != null)
            {
                emailSent = deliveryResult.Success;
                if (!deliveryResult.Success)
                {
                    failureCategory = deliveryResult.FailureCategory;
                    failureDetail = deliveryResult.DiagnosticMessage;
                }
            }
            else
            {
                emailSent = await _emailService.SendAdminCreatedUserEmailAsync(
                    user.Email!,
                    user.FullName,
                    displayRole,
                    setupUrl);
                if (!emailSent)
                {
                    failureCategory = EmailFailureCategories.UnexpectedError;
                    failureDetail = "SMTP server did not accept the message.";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while resending invitation email to '{Email}': {ErrorType}", user.Email, ex.GetType().Name);
            emailSent = false;
            failureCategory = EmailFailureCategories.UnexpectedError;
            failureDetail = $"Email delivery threw {ex.GetType().Name}.";
        }

        var dto = MapToUserDetailsDto(user, roles, Array.Empty<string>());
        dto.EmailSent = emailSent;
        dto.EmailDeliveryStatus = emailSent ? "Sent" : "Failed";
        dto.EmailFailureCategory = failureCategory;
        dto.EmailFailureDetail = failureDetail;

        if (!emailSent)
        {
            var reasonSuffix = !string.IsNullOrWhiteSpace(failureDetail)
                ? $" Reason [{failureCategory}]: {failureDetail}"
                : " Please check Gmail SMTP settings.";
            return ApiResponse<UserDetailsDto>.FailureResult(
                $"A new setup token was generated for {user.FullName}, but the invitation email to {user.Email} failed to send.{reasonSuffix}");
        }

        return ApiResponse<UserDetailsDto>.SuccessResult(
            dto,
            $"Account setup invitation email resent successfully to {user.Email}.");
    }

    public async Task<ApiResponse<UserDetailsDto>> ApprovePartnerAsync(string id, string? notes = null, string? approvedBy = null)
    {
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return ApiResponse<UserDetailsDto>.FailureResult("User not found.");
        }

        user.AccountStatus = AccountStatuses.Active;
        user.IsActive = true;
        user.RejectionReason = null;
        user.ApprovedAt = DateTime.UtcNow;
        user.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "System Administrator" : approvedBy;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.Organization != null)
        {
            user.Organization.IsActive = true;
            user.Organization.ApprovalStatus = AccountStatuses.Active;
            user.Organization.RejectionReason = null;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDetailsDto>.FailureResult("Failed to approve Partner Organization account.", errors);
        }

        _permissionService.InvalidateCache(userId: user.Id);

        var loginUrl = $"{(_emailSettings.FrontendBaseUrl?.TrimEnd('/') ?? "http://localhost:5173")}/login";
        var emailSent = false;
        try
        {
            emailSent = await _emailService.SendPartnerApprovalEmailAsync(
                user.Email ?? string.Empty,
                user.FullName,
                user.Organization?.Name ?? "Partner Organization",
                loginUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Non-fatal error while sending partner approval email to {Email}: {ErrorType}", user.Email, ex.GetType().Name);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        var dto = MapToUserDetailsDto(user, roles, permissions);
        dto.EmailSent = emailSent;
        dto.EmailDeliveryStatus = emailSent ? "Sent" : "Failed";

        return ApiResponse<UserDetailsDto>.SuccessResult(
            dto,
            $"Partner Organization account '{user.FullName}' has been approved and activated.");
    }

    public async Task<ApiResponse<UserDetailsDto>> RejectPartnerAsync(string id, string? reason = null, string? reviewedBy = null)
    {
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return ApiResponse<UserDetailsDto>.FailureResult("User not found.");
        }

        var rejectionReason = string.IsNullOrWhiteSpace(reason)
            ? "Registration did not meet CareLink Partner verification requirements."
            : reason.Trim();

        user.AccountStatus = AccountStatuses.Rejected;
        user.IsActive = false;
        user.RejectionReason = rejectionReason;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.Organization != null)
        {
            user.Organization.IsActive = false;
            user.Organization.ApprovalStatus = AccountStatuses.Rejected;
            user.Organization.RejectionReason = rejectionReason;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDetailsDto>.FailureResult("Failed to reject Partner Organization account.", errors);
        }

        _permissionService.InvalidateCache(userId: user.Id);

        var emailSent = false;
        try
        {
            emailSent = await _emailService.SendPartnerRejectionEmailAsync(
                user.Email ?? string.Empty,
                user.FullName,
                user.Organization?.Name ?? "Partner Organization",
                rejectionReason);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Non-fatal error while sending partner rejection email to {Email}: {ErrorType}", user.Email, ex.GetType().Name);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var dto = MapToUserDetailsDto(user, roles, Array.Empty<string>());
        dto.EmailSent = emailSent;
        dto.EmailDeliveryStatus = emailSent ? "Sent" : "Failed";

        return ApiResponse<UserDetailsDto>.SuccessResult(
            dto,
            $"Partner Organization account '{user.FullName}' has been rejected.");
    }

    public async Task<ApiResponse<UserDetailsDto>> UpdateUserAsync(string id, UpdateUserRequestDto request)
    {
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return ApiResponse<UserDetailsDto>.FailureResult("User not found.");
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim() ?? user.PhoneNumber;
        user.NIC = request.NIC?.Trim() ?? user.NIC;
        user.Address = request.Address?.Trim() ?? user.Address;
        if (request.OrganizationId.HasValue)
        {
            user.OrganizationId = request.OrganizationId;
        }
        if (request.Skills != null)
        {
            user.Skills = request.Skills.Trim();
        }
        if (request.Availability != null)
        {
            user.Availability = request.Availability.Trim();
        }
        if (request.PreferredLocation != null)
        {
            user.PreferredLocation = request.PreferredLocation.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.AccountStatus) && AccountStatuses.IsValid(request.AccountStatus))
        {
            user.AccountStatus = request.AccountStatus.Trim();
            user.IsActive = string.Equals(user.AccountStatus, AccountStatuses.Active, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(user.AccountStatus, AccountStatuses.PendingPasswordSetup, StringComparison.OrdinalIgnoreCase);
        }
        else if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
            user.AccountStatus = request.IsActive.Value
                ? (user.RequiresPasswordSetup ? AccountStatuses.PendingPasswordSetup : AccountStatuses.Active)
                : AccountStatuses.Disabled;
        }

        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDetailsDto>.FailureResult("Failed to update user.", errors);
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (UserRoles.TryNormalizeRoleName(request.Role, out var newRole))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(newRole))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!await _roleManager.RoleExistsAsync(newRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(newRole));
                    }
                    await _userManager.AddToRoleAsync(user, newRole);
                }
            }
        }

        _permissionService.InvalidateCache(userId: user.Id);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        var dto = MapToUserDetailsDto(user, roles, permissions);

        return ApiResponse<UserDetailsDto>.SuccessResult(dto, "User updated successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateUserStatusAsync(string id, bool isActive)
    {
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return ApiResponse<bool>.FailureResult("User not found.");
        }

        if (!isActive)
        {
            if (user.Email?.Equals("admin@carelink.org", StringComparison.OrdinalIgnoreCase) == true)
            {
                return ApiResponse<bool>.FailureResult("The default system administrator account cannot be disabled.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any(UserRoles.IsAdminRole))
            {
                var adminUsers = (await _userManager.GetUsersInRoleAsync(UserRoles.Admin)).ToList();
                var systemAdminUsers = (await _userManager.GetUsersInRoleAsync(UserRoles.SystemAdmin)).ToList();
                var remainingActiveAdmins = adminUsers.Concat(systemAdminUsers)
                    .DistinctBy(u => u.Id)
                    .Count(u => u.Id != id && u.IsActive);

                if (remainingActiveAdmins == 0)
                {
                    return ApiResponse<bool>.FailureResult("Cannot disable the last active administrator.");
                }
            }
        }

        var wasPendingPartner = string.Equals(user.AccountStatus, AccountStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase);

        user.IsActive = isActive;
        user.AccountStatus = isActive
            ? (user.RequiresPasswordSetup ? AccountStatuses.PendingPasswordSetup : AccountStatuses.Active)
            : AccountStatuses.Disabled;

        if (isActive && !user.RequiresPasswordSetup)
        {
            user.RejectionReason = null;
            user.ApprovedAt ??= DateTime.UtcNow;
            if (user.Organization != null)
            {
                user.Organization.IsActive = true;
                user.Organization.ApprovalStatus = AccountStatuses.Active;
            }
        }
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponse<bool>.FailureResult("Failed to update user status.");
        }

        _permissionService.InvalidateCache(userId: user.Id);

        if (wasPendingPartner && isActive)
        {
            var loginUrl = $"{(_emailSettings.FrontendBaseUrl?.TrimEnd('/') ?? "http://localhost:5173")}/login";
            try
            {
                await _emailService.SendPartnerApprovalEmailAsync(
                    user.Email ?? string.Empty,
                    user.FullName,
                    user.Organization?.Name ?? "Partner Organization",
                    loginUrl);
            }
            catch
            {
                // Non-fatal
            }
        }

        var statusText = isActive ? "activated" : "deactivated";
        return ApiResponse<bool>.SuccessResult(true, $"User account {statusText} successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(UserRoles.SystemAdmin))
        {
            var systemAdmins = await _userManager.GetUsersInRoleAsync(UserRoles.SystemAdmin);
            if (systemAdmins.Count <= 1)
            {
                return ApiResponse<bool>.FailureResult("Cannot delete the last System Administrator account.");
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any(UserRoles.IsAdminRole))
        {
            var adminUsers = (await _userManager.GetUsersInRoleAsync(UserRoles.Admin)).ToList();
            var systemAdminUsers = (await _userManager.GetUsersInRoleAsync(UserRoles.SystemAdmin)).ToList();
            var remainingAdmins = adminUsers.Concat(systemAdminUsers)
                .Where(u => u.IsActive && u.Id != user.Id)
                .Select(u => u.Id)
                .Distinct()
                .Count();

            if (remainingAdmins == 0)
            {
                return ApiResponse<bool>.FailureResult("Cannot delete the last active administrator.");
            }
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponse<bool>.FailureResult("Failed to delete user.");
        }

        _permissionService.InvalidateCache(userId: user.Id);
        return ApiResponse<bool>.SuccessResult(true, "User deleted successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateUserRolesAsync(string userId, UpdateUserRolesRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult("User not found.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var validRoles = request.Roles
            .Select(NormalizeRoleName)
            .Where(UserRoles.IsValidRole)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (validRoles.Count == 0)
        {
            return ApiResponse<bool>.FailureResult("At least one valid platform role must be specified.");
        }

        var isCurrentlyAdmin = currentRoles.Any(UserRoles.IsAdminRole);
        var willBeAdmin = validRoles.Any(UserRoles.IsAdminRole);

        if (isCurrentlyAdmin && !willBeAdmin)
        {
            var adminUsers = (await _userManager.GetUsersInRoleAsync(UserRoles.Admin)).ToList();
            var systemAdminUsers = (await _userManager.GetUsersInRoleAsync(UserRoles.SystemAdmin)).ToList();
            var remainingAdmins = adminUsers.Concat(systemAdminUsers)
                .Where(u => u.IsActive && u.Id != user.Id)
                .Select(u => u.Id)
                .Distinct()
                .Count();

            if (remainingAdmins == 0)
            {
                return ApiResponse<bool>.FailureResult("Cannot remove the last active administrator from the system.");
            }
        }

        foreach (var role in validRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return ApiResponse<bool>.FailureResult("Failed to clear current user roles.");
        }

        var addResult = await _userManager.AddToRolesAsync(user, validRoles);
        if (!addResult.Succeeded)
        {
            return ApiResponse<bool>.FailureResult("Failed to assign new user roles.");
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _permissionService.InvalidateCache(userId: user.Id);

        return ApiResponse<bool>.SuccessResult(true, $"Assigned roles for user '{user.FullName}' updated to: {string.Join(", ", validRoles)}.");
    }

    public async Task<ApiResponse<IReadOnlyList<string>>> GetUserPermissionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<IReadOnlyList<string>>.FailureResult("User not found.");
        }

        var permissions = await _permissionService.GetUserEffectivePermissionsAsync(userId);
        return ApiResponse<IReadOnlyList<string>>.SuccessResult(permissions, "Effective user permissions retrieved successfully.");
    }

    private static UserDetailsDto MapToUserDetailsDto(ApplicationUser user, IList<string> roles, IList<string>? permissions = null)
    {
        var status = string.IsNullOrWhiteSpace(user.AccountStatus)
            ? (user.IsActive ? (user.RequiresPasswordSetup ? AccountStatuses.PendingPasswordSetup : AccountStatuses.Active) : AccountStatuses.Disabled)
            : user.AccountStatus;

        return new UserDetailsDto
        {
            Id = user.Id,
            UserName = user.UserName ?? user.Email ?? string.Empty,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            NIC = user.NIC,
            Address = user.Address,
            Roles = roles,
            Permissions = permissions ?? new List<string>(),
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name,
            OrganizationType = user.Organization?.Type,
            RegistrationNumber = user.Organization?.RegistrationNumber,
            IsActive = user.IsActive,
            AccountStatus = status,
            RejectionReason = user.RejectionReason,
            ApprovedAt = user.ApprovedAt,
            ApprovedBy = user.ApprovedBy,
            Skills = user.Skills,
            Availability = user.Availability,
            PreferredLocation = user.PreferredLocation,
            RequiresPasswordSetup = user.RequiresPasswordSetup,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.UpdatedAt,
            LastActive = FormatLastActive(user.UpdatedAt ?? user.CreatedAt)
        };
    }

    private static string FormatRoleDisplayName(string role)
    {
        return role switch
        {
            UserRoles.SystemAdmin => "System Administrator",
            UserRoles.Admin => "System Administrator",
            UserRoles.EmergencyCoordinator => "Emergency Coordinator",
            UserRoles.ResourceManager => "Resource Manager",
            UserRoles.LogisticsManager => "Resource Manager",
            UserRoles.ShelterAdministrator => "Shelter Administrator",
            UserRoles.ShelterAdmin => "Shelter Administrator",
            UserRoles.VolunteerCoordinator => "Volunteer Coordinator",
            UserRoles.CaseManager => "Case Manager",
            UserRoles.IncidentManager => "Case Manager",
            UserRoles.FundingOfficer => "Funding Officer",
            UserRoles.Donor => "Funding Officer",
            UserRoles.PartnerOfficer => "Partner Officer",
            UserRoles.Volunteer => "Volunteer",
            UserRoles.Citizen => "Citizen",
            _ => role
        };
    }

    private static string FormatLastActive(DateTime? dateTime)
    {
        if (!dateTime.HasValue) return "Never";
        var span = DateTime.UtcNow - dateTime.Value;
        if (span.TotalMinutes < 2) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hr ago";
        if (span.TotalDays < 2) return "Yesterday";
        if (span.TotalDays < 30) return $"{(int)span.TotalDays} days ago";
        return dateTime.Value.ToString("MMM dd, yyyy");
    }

    private static string NormalizeRoleName(string role)
    {
        return UserRoles.TryNormalizeRoleName(role, out var normalized)
            ? normalized
            : role.Trim();
    }
}
