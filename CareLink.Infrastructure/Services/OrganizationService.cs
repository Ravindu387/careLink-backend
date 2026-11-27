using CareLink.Application.Common;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Organizations;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareLink.Infrastructure.Services;

public class OrganizationService : IOrganizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<OrganizationService> _logger;
    private readonly UserManager<ApplicationUser>? _userManager;
    private readonly RoleManager<IdentityRole>? _roleManager;
    private readonly IPermissionService? _permissionService;
    private readonly IEmailService? _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly IOrganizationDocumentStorageService _documentStorageService;

    public OrganizationService(
        ApplicationDbContext context,
        IAuditLogService auditLogService,
        ILogger<OrganizationService> logger,
        UserManager<ApplicationUser>? userManager = null,
        RoleManager<IdentityRole>? roleManager = null,
        IPermissionService? permissionService = null,
        IEmailService? emailService = null,
        IOptions<EmailSettings>? emailSettings = null,
        IOrganizationDocumentStorageService? documentStorageService = null)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _permissionService = permissionService;
        _emailService = emailService;
        _emailSettings = emailSettings?.Value ?? new EmailSettings();
        _documentStorageService = documentStorageService ?? new CareLink.Application.Services.OrganizationDocumentStorageService();
    }

    public async Task<ApiResponse<PaginatedListDto<OrganizationDto>>> GetOrganizationsAsync(OrganizationQueryDto query)
    {
        var orgsQuery = _context.Organizations
            .Include(o => o.Documents)
            .Include(o => o.Users)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            orgsQuery = orgsQuery.Where(o =>
                o.Name.ToLower().Contains(search) ||
                (o.RegistrationNumber != null && o.RegistrationNumber.ToLower().Contains(search)) ||
                (o.ContactEmail != null && o.ContactEmail.ToLower().Contains(search)) ||
                (o.RepresentativeFullName != null && o.RepresentativeFullName.ToLower().Contains(search)) ||
                (o.District != null && o.District.ToLower().Contains(search)) ||
                o.Type.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            if (query.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                orgsQuery = orgsQuery.Where(o => o.IsActive && o.ApprovalStatus == AccountStatuses.Active);
            else if (query.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                     query.Status.Equals("Pending Review", StringComparison.OrdinalIgnoreCase))
                orgsQuery = orgsQuery.Where(o => o.ApprovalStatus == AccountStatuses.PendingApproval || o.ApprovalStatus == AccountStatuses.AdditionalInfoRequested);
            else if (query.Status.Equals("Suspended", StringComparison.OrdinalIgnoreCase) ||
                     query.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                orgsQuery = orgsQuery.Where(o => !o.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Type) && !query.Type.Equals("All types", StringComparison.OrdinalIgnoreCase))
        {
            orgsQuery = orgsQuery.Where(o => o.Type.ToLower() == query.Type.Trim().ToLower());
        }

        var totalCount = await orgsQuery.CountAsync();
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));

        var rawList = await orgsQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = rawList.Select((org, index) =>
        {
            var code = $"ORG-{(index + 1 + (pageNumber - 1) * pageSize):D3}";
            return MapToOrganizationDto(org, code);
        }).ToList();

        var result = new PaginatedListDto<OrganizationDto>
        {
            Items = dtoList,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        return ApiResponse<PaginatedListDto<OrganizationDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<OrganizationStatsDto>> GetStatsAsync()
    {
        var allOrgs = await _context.Organizations.AsNoTracking().ToListAsync();
        var total = allOrgs.Count;
        var active = allOrgs.Count(o => o.IsActive && string.Equals(o.ApprovalStatus, AccountStatuses.Active, StringComparison.OrdinalIgnoreCase));
        var pending = allOrgs.Count(o =>
            string.Equals(o.ApprovalStatus, AccountStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(o.ApprovalStatus, AccountStatuses.AdditionalInfoRequested, StringComparison.OrdinalIgnoreCase));
        var additionalInfo = allOrgs.Count(o => string.Equals(o.ApprovalStatus, AccountStatuses.AdditionalInfoRequested, StringComparison.OrdinalIgnoreCase));
        var rejected = allOrgs.Count(o => string.Equals(o.ApprovalStatus, AccountStatuses.Rejected, StringComparison.OrdinalIgnoreCase));
        var suspended = allOrgs.Count(o => !o.IsActive && !string.Equals(o.ApprovalStatus, AccountStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase) && !string.Equals(o.ApprovalStatus, AccountStatuses.AdditionalInfoRequested, StringComparison.OrdinalIgnoreCase));

        var stats = new OrganizationStatsDto
        {
            TotalOrganizations = total,
            ActiveOrganizations = active,
            PendingOrganizations = pending,
            SuspendedOrganizations = suspended,
            VerifiedOrganizations = active,
            AdditionalInfoOrganizations = additionalInfo,
            RejectedOrganizations = rejected
        };

        return ApiResponse<OrganizationStatsDto>.SuccessResult(stats);
    }

    public async Task<ApiResponse<OrganizationDto>> GetByIdAsync(Guid id)
    {
        var org = await _context.Organizations
            .Include(o => o.Documents)
            .Include(o => o.Users)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (org == null)
            return ApiResponse<OrganizationDto>.FailureResult("Organization not found.");

        var dto = MapToOrganizationDto(org, $"ORG-{org.Id.ToString()[..6].ToUpper()}");
        return ApiResponse<OrganizationDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<OrganizationDto>> CreateOrganizationAsync(CreateOrganizationRequestDto request, string actor)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<OrganizationDto>.FailureResult("Organization name is required.");

        var exists = await _context.Organizations.AnyAsync(o => o.Name.ToLower() == request.Name.Trim().ToLower());
        if (exists)
            return ApiResponse<OrganizationDto>.FailureResult("An organization with this name already exists.");

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Type = string.IsNullOrWhiteSpace(request.Type) ? "Local NGO" : request.Type.Trim(),
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            ContactEmail = request.ContactEmail?.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            Address = request.Address?.Trim(),
            District = request.District?.Trim(),
            Province = request.Province?.Trim(),
            Description = request.Description?.Trim(),
            AreasOfOperation = request.Scope,
            IsActive = true,
            ApprovalStatus = AccountStatuses.Active,
            ReviewedBy = actor,
            ReviewedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Organization '{OrgName}' created by {Actor}", org.Name, actor);

        var dto = MapToOrganizationDto(org, $"ORG-{org.Id.ToString()[..6].ToUpper()}");
        return ApiResponse<OrganizationDto>.SuccessResult(dto, "Organization created successfully.");
    }

    public async Task<ApiResponse<OrganizationDto>> UpdateOrganizationAsync(Guid id, UpdateOrganizationRequestDto request, string actor)
    {
        var org = await _context.Organizations
            .Include(o => o.Documents)
            .Include(o => o.Users)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (org == null)
            return ApiResponse<OrganizationDto>.FailureResult("Organization not found.");

        org.Name = string.IsNullOrWhiteSpace(request.Name) ? org.Name : request.Name.Trim();
        org.Type = string.IsNullOrWhiteSpace(request.Type) ? org.Type : request.Type.Trim();
        org.RegistrationNumber = request.RegistrationNumber?.Trim() ?? org.RegistrationNumber;
        org.ContactEmail = request.ContactEmail?.Trim() ?? org.ContactEmail;
        org.ContactPhone = request.ContactPhone?.Trim() ?? org.ContactPhone;
        org.Address = request.Address?.Trim() ?? org.Address;
        org.District = request.District?.Trim() ?? org.District;
        org.Province = request.Province?.Trim() ?? org.Province;
        org.Description = request.Description?.Trim() ?? org.Description;
        if (!string.IsNullOrWhiteSpace(request.Scope))
        {
            org.AreasOfOperation = request.Scope.Trim();
        }
        org.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var dto = MapToOrganizationDto(org, $"ORG-{org.Id.ToString()[..6].ToUpper()}");
        return ApiResponse<OrganizationDto>.SuccessResult(dto, "Organization details updated.");
    }

    public async Task<ApiResponse<bool>> UpdateStatusAsync(Guid id, string status, string actor)
    {
        var org = await _context.Organizations
            .Include(o => o.Users)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (org == null)
            return ApiResponse<bool>.FailureResult("Organization not found.");

        if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            var reviewResult = await ReviewOrganizationAsync(
                id,
                new OrganizationReviewDecisionRequestDto
                {
                    Decision = "Approve",
                    Rationale = $"Organisation activated and verified by {actor}."
                },
                actor);
            return reviewResult.Success
                ? ApiResponse<bool>.SuccessResult(true, "Organization approved and activated.")
                : ApiResponse<bool>.FailureResult(reviewResult.Message);
        }

        org.IsActive = false;
        org.ApprovalStatus = status.Equals("Pending", StringComparison.OrdinalIgnoreCase)
            ? AccountStatuses.PendingApproval
            : AccountStatuses.Disabled;
        org.UpdatedAt = DateTime.UtcNow;

        foreach (var member in org.Users)
        {
            member.IsActive = false;
            member.AccountStatus = org.ApprovalStatus;
            member.UpdatedAt = DateTime.UtcNow;
            _permissionService?.InvalidateCache(userId: member.Id);
        }

        await _context.SaveChangesAsync();

        try
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
            {
                Action = $"Organisation status changed to {status}",
                Actor = actor,
                Record = org.Name,
                Change = $"Status → {status}",
                Rationale = $"Administrative status update by {actor}.",
                Type = "Organisation",
                Environment = "Production"
            });
        }
        catch
        {
            // Non-fatal
        }

        _logger.LogInformation("Organization '{OrgName}' status changed to {Status} by {Actor}", org.Name, status, actor);
        return ApiResponse<bool>.SuccessResult(true, $"Organization status updated to {status}.");
    }

    public async Task<ApiResponse<bool>> ApproveVerificationAsync(Guid id, string actor)
    {
        var result = await ReviewOrganizationAsync(
            id,
            new OrganizationReviewDecisionRequestDto
            {
                Decision = "Approve",
                Rationale = "Organisation credentials verified and approved by administrator."
            },
            actor);

        if (!result.Success)
        {
            return ApiResponse<bool>.FailureResult(result.Message);
        }

        return ApiResponse<bool>.SuccessResult(true, result.Message);
    }

    public async Task<ApiResponse<OrganizationDto>> ReviewOrganizationAsync(
        Guid id,
        OrganizationReviewDecisionRequestDto request,
        string actor)
    {
        var org = await _context.Organizations
            .Include(o => o.Documents)
            .Include(o => o.Users)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (org == null)
        {
            return ApiResponse<OrganizationDto>.FailureResult("Organisation not found.");
        }

        var decision = request.Decision?.Trim() ?? "Approve";
        var rationale = string.IsNullOrWhiteSpace(request.Rationale)
            ? "Reviewed by CareLink System Administrator."
            : request.Rationale.Trim();
        var reviewer = string.IsNullOrWhiteSpace(actor) ? "System Administrator" : actor;
        var previousStatus = org.ApprovalStatus;

        var representativeUser = org.Users.FirstOrDefault(u => u.Id == org.RepresentativeUserId)
                                 ?? org.Users.FirstOrDefault();

        if (representativeUser == null && !string.IsNullOrWhiteSpace(org.RepresentativeUserId) && _userManager != null)
        {
            representativeUser = await _userManager.FindByIdAsync(org.RepresentativeUserId);
        }

        var recipientEmail = representativeUser?.Email
                             ?? org.RepresentativeEmail
                             ?? org.ContactEmail
                             ?? string.Empty;
        var recipientName = representativeUser?.FullName
                            ?? org.RepresentativeFullName
                            ?? org.Name;

        var emailSent = false;
        string actionName;
        string responseMessage;

        if (decision.Equals("Approve", StringComparison.OrdinalIgnoreCase) ||
            decision.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            org.IsActive = true;
            org.ApprovalStatus = AccountStatuses.Active;
            org.RejectionReason = null;
            org.ReviewedBy = reviewer;
            org.ReviewedAt = DateTime.UtcNow;
            org.UpdatedAt = DateTime.UtcNow;

            var usersToActivate = org.Users.ToList();
            if (representativeUser != null && !usersToActivate.Any(u => u.Id == representativeUser.Id))
            {
                usersToActivate.Add(representativeUser);
            }

            foreach (var user in usersToActivate)
            {
                user.IsActive = true;
                user.AccountStatus = AccountStatuses.Active;
                user.RejectionReason = null;
                user.ApprovedBy = reviewer;
                user.ApprovedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                if (_userManager != null && _roleManager != null)
                {
                    if (!await _roleManager.RoleExistsAsync(UserRoles.PartnerOfficer))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.PartnerOfficer));
                    }

                    if (!await _userManager.IsInRoleAsync(user, UserRoles.PartnerOfficer))
                    {
                        await _userManager.AddToRoleAsync(user, UserRoles.PartnerOfficer);
                    }
                }

                _permissionService?.InvalidateCache(userId: user.Id);
            }

            await _context.SaveChangesAsync();

            if (_emailService != null && !string.IsNullOrWhiteSpace(recipientEmail))
            {
                var loginUrl = $"{(_emailSettings.FrontendBaseUrl?.TrimEnd('/') ?? "http://localhost:5173")}/login";
                try
                {
                    emailSent = await _emailService.SendPartnerApprovalEmailAsync(
                        recipientEmail,
                        recipientName,
                        org.Name,
                        loginUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Non-fatal error sending approval email to {Email}: {ErrorType}", recipientEmail, ex.GetType().Name);
                }
            }

            actionName = "Partner organisation approved";
            responseMessage = $"Partner Organisation '{org.Name}' has been approved and activated.";
        }
        else if (decision.Equals("Reject", StringComparison.OrdinalIgnoreCase) ||
                 decision.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            org.IsActive = false;
            org.ApprovalStatus = AccountStatuses.Rejected;
            org.RejectionReason = rationale;
            org.ReviewedBy = reviewer;
            org.ReviewedAt = DateTime.UtcNow;
            org.UpdatedAt = DateTime.UtcNow;

            var usersToReject = org.Users.ToList();
            if (representativeUser != null && !usersToReject.Any(u => u.Id == representativeUser.Id))
            {
                usersToReject.Add(representativeUser);
            }

            foreach (var user in usersToReject)
            {
                user.IsActive = false;
                user.AccountStatus = AccountStatuses.Rejected;
                user.RejectionReason = rationale;
                user.UpdatedAt = DateTime.UtcNow;
                _permissionService?.InvalidateCache(userId: user.Id);
            }

            await _context.SaveChangesAsync();

            if (_emailService != null && !string.IsNullOrWhiteSpace(recipientEmail))
            {
                try
                {
                    emailSent = await _emailService.SendPartnerRejectionEmailAsync(
                        recipientEmail,
                        recipientName,
                        org.Name,
                        rationale);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Non-fatal error sending rejection email to {Email}: {ErrorType}", recipientEmail, ex.GetType().Name);
                }
            }

            actionName = "Partner organisation rejected";
            responseMessage = $"Partner Organisation '{org.Name}' has been rejected.";
        }
        else if (decision.Equals("RequestInfo", StringComparison.OrdinalIgnoreCase) ||
                 decision.Equals("Additional Info", StringComparison.OrdinalIgnoreCase) ||
                 decision.Equals("AdditionalInfoRequested", StringComparison.OrdinalIgnoreCase))
        {
            org.IsActive = false;
            org.ApprovalStatus = AccountStatuses.AdditionalInfoRequested;
            org.AdditionalInfoRequest = rationale;
            org.AdditionalInfoRequestedAt = DateTime.UtcNow;
            org.ReviewedBy = reviewer;
            org.ReviewedAt = DateTime.UtcNow;
            org.UpdatedAt = DateTime.UtcNow;

            var usersToKeepPending = org.Users.ToList();
            if (representativeUser != null && !usersToKeepPending.Any(u => u.Id == representativeUser.Id))
            {
                usersToKeepPending.Add(representativeUser);
            }

            foreach (var user in usersToKeepPending)
            {
                user.IsActive = false;
                user.AccountStatus = AccountStatuses.PendingApproval;
                user.UpdatedAt = DateTime.UtcNow;
                _permissionService?.InvalidateCache(userId: user.Id);
            }

            await _context.SaveChangesAsync();

            if (_emailService != null && !string.IsNullOrWhiteSpace(recipientEmail))
            {
                try
                {
                    emailSent = await _emailService.SendPartnerAdditionalInfoEmailAsync(
                        recipientEmail,
                        recipientName,
                        org.Name,
                        rationale);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Non-fatal error sending additional info email to {Email}: {ErrorType}", recipientEmail, ex.GetType().Name);
                }
            }

            actionName = "Partner organisation additional information requested";
            responseMessage = $"Additional verification information requested from '{org.Name}'.";
        }
        else
        {
            return ApiResponse<OrganizationDto>.FailureResult($"Unsupported review decision '{decision}'. Use Approve, Reject, or RequestInfo.");
        }

        try
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
            {
                Action = actionName,
                Actor = reviewer,
                Record = org.Name,
                Change = $"{previousStatus} → {org.ApprovalStatus}",
                Rationale = rationale,
                Type = "Organisation",
                Environment = "Production"
            });
        }
        catch
        {
            // Non-fatal
        }

        var dto = MapToOrganizationDto(org, $"ORG-{org.Id.ToString()[..6].ToUpper()}");
        dto.EmailSent = emailSent;

        return ApiResponse<OrganizationDto>.SuccessResult(dto, responseMessage);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)?> GetDocumentForDownloadAsync(
        Guid organizationId,
        Guid documentId)
    {
        var doc = await _context.OrganizationDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.OrganizationId == organizationId && d.Id == documentId);

        if (doc == null)
        {
            return null;
        }

        return await _documentStorageService.ReadDocumentAsync(doc);
    }

    public async Task<ApiResponse<bool>> DeleteOrganizationAsync(Guid id, string actor)
    {
        var org = await _context.Organizations.FindAsync(id);
        if (org == null)
            return ApiResponse<bool>.FailureResult("Organization not found.");

        var hasUsers = await _context.Users.AnyAsync(u => u.OrganizationId == id);
        if (hasUsers)
            return ApiResponse<bool>.FailureResult("Cannot delete organization with assigned users. Reassign or remove members first.");

        _context.Organizations.Remove(org);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Organization '{OrgName}' deleted by {Actor}", org.Name, actor);
        return ApiResponse<bool>.SuccessResult(true, "Organization removed successfully.");
    }

    public Task<ApiResponse<IReadOnlyList<string>>> GetOrganizationTypesAsync()
    {
        IReadOnlyList<string> types = new List<string>
        {
            "International NGO",
            "Local NGO",
            "Government Organisation",
            "UN Agency",
            "Charity / Non-profit Organisation",
            "Community-Based Organisation",
            "Religious / Faith-Based Organisation",
            "Private Humanitarian Organisation",
            "Other"
        };
        return Task.FromResult(ApiResponse<IReadOnlyList<string>>.SuccessResult(types));
    }

    private static OrganizationDto MapToOrganizationDto(Organization org, string code)
    {
        var repUser = org.Users?.FirstOrDefault(u => u.Id == org.RepresentativeUserId)
                      ?? org.Users?.FirstOrDefault();

        var verificationStatus = org.ApprovalStatus switch
        {
            AccountStatuses.PendingApproval => "Pending Review",
            AccountStatuses.AdditionalInfoRequested => "Additional Info",
            AccountStatuses.Rejected => "Rejected",
            _ => org.IsActive ? "Verified" : "Pending Review"
        };

        var uiStatus = org.ApprovalStatus switch
        {
            AccountStatuses.PendingApproval => "Pending",
            AccountStatuses.AdditionalInfoRequested => "Pending",
            AccountStatuses.Rejected => "Suspended",
            AccountStatuses.Disabled => "Suspended",
            _ => org.IsActive ? "Active" : "Suspended"
        };

        var scopeDisplay = !string.IsNullOrWhiteSpace(org.AreasOfOperation)
            ? org.AreasOfOperation
            : (!string.IsNullOrWhiteSpace(org.District)
                ? $"{org.District}{(!string.IsNullOrWhiteSpace(org.Province) ? $", {org.Province}" : "")}"
                : (org.Type.Contains("Government", StringComparison.OrdinalIgnoreCase) ? "National" : "Greater Riverton"));

        return new OrganizationDto
        {
            Id = org.Id,
            Code = code,
            Name = org.Name,
            Type = org.Type,
            OtherOrganizationType = org.OtherOrganizationType,
            Scope = scopeDisplay,
            RegistrationCountry = string.IsNullOrWhiteSpace(org.RegistrationCountry) ? "Sri Lanka" : org.RegistrationCountry,
            RegistrationNumber = org.RegistrationNumber,
            YearEstablished = org.YearEstablished,
            Website = org.Website,
            ContactEmail = org.ContactEmail ?? repUser?.Email,
            ContactPhone = org.ContactPhone ?? repUser?.PhoneNumber,
            SecondaryContactPhone = org.SecondaryContactPhone,
            Address = org.Address,
            District = org.District,
            Province = org.Province,
            Description = !string.IsNullOrWhiteSpace(org.Description)
                ? org.Description
                : $"Authorized partner entity operating under {org.Type} standards.",
            AreasOfOperation = SplitCsvList(org.AreasOfOperation),
            AreasOfAssistance = SplitCsvList(org.AreasOfAssistance),
            OtherAreaOfAssistance = org.OtherAreaOfAssistance,
            OperationalCapacity = org.OperationalCapacity,
            EmergencyResponseAvailability = org.EmergencyResponseAvailability,
            AdditionalResources = org.AdditionalResources,
            RepresentativeUserId = org.RepresentativeUserId ?? repUser?.Id,
            RepresentativeFirstName = org.RepresentativeFirstName,
            RepresentativeLastName = org.RepresentativeLastName,
            RepresentativeFullName = org.RepresentativeFullName ?? repUser?.FullName,
            RepresentativeDesignation = org.RepresentativeDesignation ?? repUser?.Designation,
            RepresentativeEmail = org.RepresentativeEmail ?? repUser?.Email,
            RepresentativePhone = org.RepresentativePhone ?? repUser?.PhoneNumber,
            RepresentativeAlternativePhone = org.RepresentativeAlternativePhone ?? repUser?.AlternativePhoneNumber,
            MembersCount = org.Users != null && org.Users.Count > 0 ? org.Users.Count : 1,
            VerificationStatus = verificationStatus,
            Status = uiStatus,
            ApprovalStatus = org.ApprovalStatus,
            IsActive = org.IsActive,
            RejectionReason = org.RejectionReason,
            AdditionalInfoRequest = org.AdditionalInfoRequest,
            AdditionalInfoResponse = org.AdditionalInfoResponse,
            ReviewedBy = org.ReviewedBy,
            ReviewedAt = org.ReviewedAt,
            CreatedAt = org.CreatedAt,
            Documents = (org.Documents ?? Array.Empty<OrganizationDocument>())
                .OrderBy(d => d.UploadedAt)
                .Select(d => new OrganizationDocumentDto
                {
                    Id = d.Id,
                    OrganizationId = d.OrganizationId,
                    Category = d.DocumentCategory,
                    Label = d.DocumentLabel,
                    OriginalFileName = d.OriginalFileName,
                    ContentType = d.ContentType,
                    FileSizeBytes = d.FileSizeBytes,
                    FileSizeDisplay = d.FileSizeDisplay,
                    UploadedAt = d.UploadedAt
                }).ToList()
        };
    }

    private static List<string> SplitCsvList(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new List<string>();
        }

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}
