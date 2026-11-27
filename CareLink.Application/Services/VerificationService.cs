using System.Collections.Concurrent;
using System.Text;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Organizations;
using CareLink.Application.DTOs.Users;
using CareLink.Application.DTOs.Verification;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace CareLink.Application.Services;

public class VerificationService : IVerificationService
{
    private readonly ConcurrentDictionary<string, VerificationRequest> _requests = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, string> _orgToRequestId = new();
    private readonly IAuditLogService _auditLogService;
    private readonly IServiceScopeFactory? _scopeFactory;
    private int _idSequence = 2050;

    public VerificationService(
        IAuditLogService auditLogService,
        IServiceScopeFactory? scopeFactory = null)
    {
        _auditLogService = auditLogService;
        _scopeFactory = scopeFactory;
        InitializeDefaultRequests();
    }

    private void InitializeDefaultRequests()
    {
        var now = DateTime.UtcNow;

        var seedList = new List<VerificationRequest>
        {
            new()
            {
                Id = "VER-2048",
                Applicant = "Global Relief Network",
                Type = "Partner Organisation",
                Evidence = "Registration Certificate · Organisation Profile",
                Priority = "High",
                Status = "Pending Review",
                SubmittedAt = now.AddHours(-2),
                ApplicantEmail = "partnerships@globalrelief.org",
                ApplicantPhone = "+94112345678",
                OrganizationName = "Global Relief Network",
                OrganizationType = "International NGO",
                RegistrationCountry = "Sri Lanka",
                RegistrationNumber = "INGO-2021-408",
                YearEstablished = 2014,
                Website = "https://globalrelief.org",
                Address = "42 Horton Place, Colombo 07",
                District = "Colombo",
                Province = "Western Province",
                Description = "Multi-district flood, landslide, and emergency shelter relief coordination.",
                AreasOfOperation = "Colombo, Gampaha, Kalutara, Ratnapura",
                AreasOfAssistance = "Food Distribution, Emergency Medical Assistance, Shelter and Accommodation, Water and Sanitation",
                OperationalCapacity = "3 mobile medical teams, 4 regional relief warehouses, and 120 trained emergency responders.",
                EmergencyResponseAvailability = "Available for emergency response",
                AdditionalResources = "6 high-clearance relief trucks, 2 mobile water purification units",
                RepresentativeFullName = "Amara Wijesinghe",
                RepresentativeDesignation = "Country Director",
                RepresentativeEmail = "amara.w@globalrelief.org",
                RepresentativePhone = "+94771234567",
                LoginEmail = "partnerships@globalrelief.org",
                Documents =
                [
                    new() { Id = "DOC-101", Category = "RegistrationCertificate", Name = "Organisation / NGO Registration Certificate", OriginalFileName = "GRN_NGO_Certificate_2026.pdf", Type = "PDF", Size = "2.4 MB", Status = "Uploaded", UploadedAt = now.AddHours(-2) },
                    new() { Id = "DOC-102", Category = "OrganisationProfile", Name = "Organisation Profile / Constitution", OriginalFileName = "GRN_Constitution_Profile.pdf", Type = "PDF", Size = "1.1 MB", Status = "Uploaded", UploadedAt = now.AddHours(-2) },
                    new() { Id = "DOC-103", Category = "AuthorisationLetter", Name = "Authorisation Letter for Representative", OriginalFileName = "GRN_Director_Authorisation.pdf", Type = "PDF", Size = "890 KB", Status = "Uploaded", UploadedAt = now.AddHours(-2) }
                ]
            },
            new()
            {
                Id = "VER-2039",
                Applicant = "Riverton Medical Collective",
                Type = "Partner Organisation",
                Evidence = "Registration Certificate · Authorisation Letter",
                Priority = "Standard",
                Status = "Additional Info",
                SubmittedAt = now.AddDays(-1).AddHours(-2),
                ApplicantEmail = "contact@rivertonmedical.org",
                ApplicantPhone = "+94114567890",
                OrganizationName = "Riverton Medical Collective",
                OrganizationType = "Charity / Non-profit Organisation",
                RegistrationCountry = "Sri Lanka",
                RegistrationNumber = "MED-NP-2023-19",
                Address = "18 Hospital Road, Kalutara",
                District = "Kalutara",
                Province = "Western Province",
                Description = "Emergency trauma and mobile clinic deployment for flood-affected families.",
                AreasOfOperation = "Kalutara, Galle",
                AreasOfAssistance = "Emergency Medical Assistance, Child Protection",
                OperationalCapacity = "25 volunteer doctors and paramedics with portable triage kits.",
                EmergencyResponseAvailability = "Limited availability",
                RepresentativeFullName = "Dr. Ruwan Jayawardena",
                RepresentativeDesignation = "Chief Medical Coordinator",
                RepresentativeEmail = "ruwan@rivertonmedical.org",
                RepresentativePhone = "+94714567890",
                LoginEmail = "contact@rivertonmedical.org",
                AdditionalInfoRequest = "Registration certificate scan was partially cropped. Please upload a clear full-page copy.",
                DecisionRationale = "Registration certificate scan was partially cropped. Please upload a clear full-page copy.",
                ReviewedBy = "System Administrator",
                ReviewedAt = now.AddHours(-4),
                Documents =
                [
                    new() { Id = "DOC-106", Category = "RegistrationCertificate", Name = "Organisation / NGO Registration Certificate", OriginalFileName = "Riverton_Registration_Cert.pdf", Type = "PDF", Size = "3.2 MB", Status = "Uploaded", UploadedAt = now.AddDays(-1) },
                    new() { Id = "DOC-107", Category = "AuthorisationLetter", Name = "Authorisation Letter for Representative", OriginalFileName = "Board_Authorisation_Letter.pdf", Type = "PDF", Size = "1.8 MB", Status = "Uploaded", UploadedAt = now.AddDays(-1) }
                ]
            },
            new()
            {
                Id = "VER-2007",
                Applicant = "Highland Relief Volunteers",
                Type = "Partner Organisation",
                Evidence = "Registration Certificate",
                Priority = "Standard",
                Status = "Approved",
                SubmittedAt = now.AddDays(-6),
                ReviewedAt = now.AddHours(-2),
                ReviewedBy = "System Administrator",
                DecisionRationale = "NGO registration confirmed with district secretariat.",
                ApplicantEmail = "highland@volunteers.org",
                ApplicantPhone = "+94812345678",
                OrganizationName = "Highland Relief Volunteers",
                OrganizationType = "Community-Based Organisation",
                RegistrationCountry = "Sri Lanka",
                RegistrationNumber = "CBO-KDY-2022-88",
                Address = "105 Peradeniya Road, Kandy",
                District = "Kandy",
                Province = "Central Province",
                Description = "Landslide early response and community evacuation support.",
                AreasOfOperation = "Kandy, Nuwara Eliya, Matale",
                AreasOfAssistance = "Search and Rescue, Shelter and Accommodation, Food Distribution",
                OperationalCapacity = "85 community first responders trained in landslide evacuation.",
                EmergencyResponseAvailability = "Available for emergency response",
                RepresentativeFullName = "Kasun Bandara",
                RepresentativeDesignation = "Operations Secretary",
                RepresentativeEmail = "kasun@volunteers.org",
                RepresentativePhone = "+94812345678",
                LoginEmail = "highland@volunteers.org",
                Documents =
                [
                    new() { Id = "DOC-119", Category = "RegistrationCertificate", Name = "Organisation / NGO Registration Certificate", OriginalFileName = "Highland_CBO_Certificate.pdf", Type = "PDF", Size = "1.6 MB", Status = "Uploaded", UploadedAt = now.AddDays(-6) }
                ]
            }
        };

        foreach (var req in seedList)
        {
            _requests[req.Id] = req;
        }
    }

    public async Task<VerificationRequestDto> RegisterPartnerOrganizationRequestAsync(
        Organization organization,
        ApplicationUser representative)
    {
        var reqId = _orgToRequestId.GetOrAdd(
            organization.Id,
            _ => $"VER-{Interlocked.Increment(ref _idSequence)}");

        var mapped = BuildRequestFromOrganization(reqId, organization, representative);
        _requests[reqId] = mapped;

        return await Task.FromResult(MapToDto(mapped));
    }

    private async Task SyncDatabaseOrganizationsAsync()
    {
        if (_scopeFactory == null)
        {
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var orgService = scope.ServiceProvider.GetService<IOrganizationService>();
            if (orgService == null)
            {
                return;
            }

            var orgsResponse = await orgService.GetOrganizationsAsync(new OrganizationQueryDto
            {
                PageNumber = 1,
                PageSize = 100
            });

            if (orgsResponse.Success && orgsResponse.Data?.Items != null)
            {
                foreach (var orgDto in orgsResponse.Data.Items)
                {
                    // Skip the default seeded government organization if it has no partner representative application
                    if (orgDto.Name == "National Disaster Management Centre" &&
                        string.IsNullOrWhiteSpace(orgDto.RepresentativeEmail) &&
                        orgDto.Documents.Count == 0)
                    {
                        continue;
                    }

                    var reqId = _orgToRequestId.GetOrAdd(
                        orgDto.Id,
                        _ => $"VER-{Interlocked.Increment(ref _idSequence)}");

                    var verStatus = MapApprovalStatusToVerificationStatus(orgDto.ApprovalStatus, orgDto.IsActive);

                    var docs = orgDto.Documents.Select(d => new VerificationDocument
                    {
                        Id = d.Id.ToString(),
                        DocumentId = d.Id,
                        OrganizationId = orgDto.Id,
                        Category = d.Category,
                        Name = d.Label,
                        OriginalFileName = d.OriginalFileName,
                        Type = Path.GetExtension(d.OriginalFileName).TrimStart('.').ToUpperInvariant(),
                        ContentType = d.ContentType,
                        Size = d.FileSizeDisplay,
                        FileSizeBytes = d.FileSizeBytes,
                        Status = "Uploaded",
                        CanDownload = true,
                        UploadedAt = d.UploadedAt
                    }).ToList();

                    var evidenceSummary = docs.Count > 0
                        ? string.Join(" · ", docs.Select(d => d.Name))
                        : "Organisation Registration Application";

                    var req = new VerificationRequest
                    {
                        Id = reqId,
                        OrganizationId = orgDto.Id,
                        UserId = orgDto.RepresentativeUserId,
                        Applicant = orgDto.Name,
                        Type = "Partner Organisation",
                        Evidence = evidenceSummary,
                        Priority = "High",
                        Status = verStatus,
                        SubmittedAt = orgDto.CreatedAt,
                        ApplicantEmail = orgDto.ContactEmail ?? orgDto.RepresentativeEmail,
                        ApplicantPhone = orgDto.ContactPhone ?? orgDto.RepresentativePhone,
                        OrganizationName = orgDto.Name,
                        OrganizationType = orgDto.Type,
                        OtherOrganizationType = orgDto.OtherOrganizationType,
                        RegistrationCountry = orgDto.RegistrationCountry,
                        RegistrationNumber = orgDto.RegistrationNumber,
                        YearEstablished = orgDto.YearEstablished,
                        Website = orgDto.Website,
                        SecondaryContactPhone = orgDto.SecondaryContactPhone,
                        Address = orgDto.Address,
                        District = orgDto.District,
                        Province = orgDto.Province,
                        Description = orgDto.Description,
                        AreasOfOperation = string.Join(", ", orgDto.AreasOfOperation),
                        AreasOfAssistance = string.Join(", ", orgDto.AreasOfAssistance),
                        OtherAreaOfAssistance = orgDto.OtherAreaOfAssistance,
                        OperationalCapacity = orgDto.OperationalCapacity,
                        EmergencyResponseAvailability = orgDto.EmergencyResponseAvailability,
                        AdditionalResources = orgDto.AdditionalResources,
                        RepresentativeFullName = orgDto.RepresentativeFullName,
                        RepresentativeDesignation = orgDto.RepresentativeDesignation,
                        RepresentativeEmail = orgDto.RepresentativeEmail,
                        RepresentativePhone = orgDto.RepresentativePhone,
                        RepresentativeAlternativePhone = orgDto.RepresentativeAlternativePhone,
                        LoginEmail = orgDto.RepresentativeEmail ?? orgDto.ContactEmail,
                        AdditionalInfoRequest = orgDto.AdditionalInfoRequest,
                        AdditionalInfoResponse = orgDto.AdditionalInfoResponse,
                        ReviewedBy = orgDto.ReviewedBy,
                        ReviewedAt = orgDto.ReviewedAt,
                        DecisionRationale = orgDto.RejectionReason ?? orgDto.AdditionalInfoRequest,
                        Documents = docs
                    };

                    _requests[reqId] = req;
                }
            }
        }
        catch
        {
            // Non-fatal fallback to in-memory dictionary
        }
    }

    private static VerificationRequest BuildRequestFromOrganization(
        string reqId,
        Organization org,
        ApplicationUser? representative)
    {
        var docs = (org.Documents ?? Array.Empty<OrganizationDocument>()).Select(d => new VerificationDocument
        {
            Id = d.Id.ToString(),
            DocumentId = d.Id,
            OrganizationId = org.Id,
            Category = d.DocumentCategory,
            Name = d.DocumentLabel,
            OriginalFileName = d.OriginalFileName,
            Type = Path.GetExtension(d.OriginalFileName).TrimStart('.').ToUpperInvariant(),
            ContentType = d.ContentType,
            Size = d.FileSizeDisplay,
            FileSizeBytes = d.FileSizeBytes,
            Status = "Uploaded",
            CanDownload = true,
            UploadedAt = d.UploadedAt
        }).ToList();

        var evidenceSummary = docs.Count > 0
            ? string.Join(" · ", docs.Select(d => d.Name))
            : "Organisation Registration Application";

        return new VerificationRequest
        {
            Id = reqId,
            OrganizationId = org.Id,
            UserId = representative?.Id ?? org.RepresentativeUserId,
            Applicant = org.Name,
            Type = "Partner Organisation",
            Evidence = evidenceSummary,
            Priority = "High",
            Status = MapApprovalStatusToVerificationStatus(org.ApprovalStatus, org.IsActive),
            SubmittedAt = org.CreatedAt,
            ApplicantEmail = org.ContactEmail ?? representative?.Email,
            ApplicantPhone = org.ContactPhone ?? representative?.PhoneNumber,
            OrganizationName = org.Name,
            OrganizationType = org.Type,
            OtherOrganizationType = org.OtherOrganizationType,
            RegistrationCountry = org.RegistrationCountry,
            RegistrationNumber = org.RegistrationNumber,
            YearEstablished = org.YearEstablished,
            Website = org.Website,
            SecondaryContactPhone = org.SecondaryContactPhone,
            Address = org.Address,
            District = org.District,
            Province = org.Province,
            Description = org.Description,
            AreasOfOperation = org.AreasOfOperation,
            AreasOfAssistance = org.AreasOfAssistance,
            OtherAreaOfAssistance = org.OtherAreaOfAssistance,
            OperationalCapacity = org.OperationalCapacity,
            EmergencyResponseAvailability = org.EmergencyResponseAvailability,
            AdditionalResources = org.AdditionalResources,
            RepresentativeFullName = org.RepresentativeFullName ?? representative?.FullName,
            RepresentativeDesignation = org.RepresentativeDesignation ?? representative?.Designation,
            RepresentativeEmail = org.RepresentativeEmail ?? representative?.Email,
            RepresentativePhone = org.RepresentativePhone ?? representative?.PhoneNumber,
            RepresentativeAlternativePhone = org.RepresentativeAlternativePhone ?? representative?.AlternativePhoneNumber,
            LoginEmail = representative?.Email ?? org.RepresentativeEmail ?? org.ContactEmail,
            AdditionalInfoRequest = org.AdditionalInfoRequest,
            AdditionalInfoResponse = org.AdditionalInfoResponse,
            ReviewedBy = org.ReviewedBy,
            ReviewedAt = org.ReviewedAt,
            DecisionRationale = org.RejectionReason ?? org.AdditionalInfoRequest,
            Documents = docs
        };
    }

    private static string MapApprovalStatusToVerificationStatus(string? approvalStatus, bool isActive)
    {
        if (string.Equals(approvalStatus, AccountStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase))
        {
            return "Pending Review";
        }

        if (string.Equals(approvalStatus, AccountStatuses.AdditionalInfoRequested, StringComparison.OrdinalIgnoreCase))
        {
            return "Additional Info";
        }

        if (string.Equals(approvalStatus, AccountStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            return "Rejected";
        }

        if (isActive || string.Equals(approvalStatus, AccountStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            return "Approved";
        }

        return "Pending Review";
    }

    public async Task<ApiResponse<PaginatedListDto<VerificationRequestDto>>> GetRequestsAsync(VerificationQueryDto query)
    {
        await SyncDatabaseOrganizationsAsync();

        var items = FilterRequests(query);
        var ordered = items.OrderByDescending(r => r.SubmittedAt).ToList();

        var totalCount = ordered.Count;
        var page = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 20;

        var pagedItems = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        var result = new PaginatedListDto<VerificationRequestDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return ApiResponse<PaginatedListDto<VerificationRequestDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<VerificationRequestDto>> GetRequestByIdAsync(string id)
    {
        await SyncDatabaseOrganizationsAsync();

        if (_requests.TryGetValue(id, out var req))
        {
            return ApiResponse<VerificationRequestDto>.SuccessResult(MapToDto(req));
        }

        return ApiResponse<VerificationRequestDto>.FailureResult($"Verification request '{id}' not found.");
    }

    public async Task<ApiResponse<VerificationStatsDto>> GetStatsAsync()
    {
        await SyncDatabaseOrganizationsAsync();

        var all = _requests.Values.ToList();
        var now = DateTime.UtcNow;
        var today = now.Date;

        var totalPending = all.Count(r => r.Status.Equals("Pending Review", StringComparison.OrdinalIgnoreCase));
        var highPriority = all.Count(r => r.Status.Equals("Pending Review", StringComparison.OrdinalIgnoreCase) &&
                                          r.Priority.Equals("High", StringComparison.OrdinalIgnoreCase));
        var approvedToday = all.Count(r => r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
                                           (r.ReviewedAt.HasValue ? r.ReviewedAt.Value >= today : r.SubmittedAt >= today));
        var additionalInfo = all.Count(r => r.Status.Equals("Additional Info", StringComparison.OrdinalIgnoreCase));

        var pendingItems = all
            .Where(r => r.Status.Equals("Pending Review", StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.SubmittedAt)
            .ToList();

        var oldestPendingHours = 0;
        var oldestPendingText = "All pending requests are within fresh review window.";

        if (pendingItems.Count > 0)
        {
            var diff = now - pendingItems[0].SubmittedAt;
            oldestPendingHours = (int)diff.TotalHours;
            if (diff.TotalDays >= 1)
            {
                oldestPendingText = $"Oldest unresolved request was submitted {(int)diff.TotalDays} days ago.";
            }
            else
            {
                oldestPendingText = $"Oldest unresolved request was submitted {Math.Max(1, (int)diff.TotalHours)} hours ago.";
            }
        }

        var stats = new VerificationStatsDto
        {
            TotalPending = totalPending,
            HighPriority = highPriority,
            ApprovedToday = approvedToday,
            AdditionalInfo = additionalInfo,
            OldestPendingHours = oldestPendingHours,
            OldestPendingText = oldestPendingText,
            SlaTarget = "24 hours",
            SlaStatus = oldestPendingHours > 24 ? "SLA Attention Required" : "Within SLA: 24 hours",
            TotalRequests = all.Count
        };

        return ApiResponse<VerificationStatsDto>.SuccessResult(stats);
    }

    public async Task<ApiResponse<IReadOnlyList<string>>> GetRequestTypesAsync()
    {
        await SyncDatabaseOrganizationsAsync();

        var types = _requests.Values
            .Select(r => r.Type)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        return ApiResponse<IReadOnlyList<string>>.SuccessResult(types);
    }

    public Task<ApiResponse<VerificationRequestDto>> CreateRequestAsync(CreateVerificationRequestDto request)
    {
        var nextId = $"VER-{Interlocked.Increment(ref _idSequence)}";

        var docList = new List<VerificationDocument>();
        if (request.DocumentNames != null && request.DocumentNames.Count > 0)
        {
            for (int i = 0; i < request.DocumentNames.Count; i++)
            {
                docList.Add(new VerificationDocument
                {
                    Id = $"DOC-{nextId}-{i + 1}",
                    Name = request.DocumentNames[i].Trim(),
                    OriginalFileName = request.DocumentNames[i].Trim(),
                    Type = "PDF",
                    Size = "1.5 MB",
                    Status = "Uploaded",
                    UploadedAt = DateTime.UtcNow
                });
            }
        }
        else
        {
            docList.Add(new VerificationDocument
            {
                Id = $"DOC-{nextId}-1",
                Name = "Identity Verification Document",
                OriginalFileName = "Identity_Document.pdf",
                Type = "PDF",
                Size = "1.2 MB",
                Status = "Uploaded",
                UploadedAt = DateTime.UtcNow
            });
        }

        var evidenceSummary = string.Join(" · ", docList.Select(d => d.Name));

        var newReq = new VerificationRequest
        {
            Id = nextId,
            Applicant = request.Applicant.Trim(),
            Type = request.Type.Trim(),
            Evidence = evidenceSummary,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Standard" : request.Priority.Trim(),
            Status = "Pending Review",
            SubmittedAt = DateTime.UtcNow,
            ApplicantEmail = request.ApplicantEmail?.Trim(),
            ApplicantPhone = request.ApplicantPhone?.Trim(),
            OrganizationName = request.OrganizationName?.Trim(),
            Documents = docList
        };

        _requests[newReq.Id] = newReq;

        return Task.FromResult(ApiResponse<VerificationRequestDto>.SuccessResult(
            MapToDto(newReq), "Verification request submitted successfully."));
    }

    public async Task<ApiResponse<VerificationRequestDto>> RecordDecisionAsync(
        string id,
        VerificationDecisionRequestDto request,
        string actor)
    {
        await SyncDatabaseOrganizationsAsync();

        if (!_requests.TryGetValue(id, out var req))
        {
            return ApiResponse<VerificationRequestDto>.FailureResult($"Verification request '{id}' not found.");
        }

        var rawStatus = request.ResolvedStatus;
        var normalizedStatus = NormalizeVerificationStatus(rawStatus);
        var previousStatus = req.Status;
        var reviewer = string.IsNullOrWhiteSpace(actor) ? "System Administrator" : actor;
        var rationale = request.ResolvedRationale;

        if (string.IsNullOrWhiteSpace(rationale))
        {
            return ApiResponse<VerificationRequestDto>.FailureResult("Please provide a decision rationale before continuing.");
        }

        var emailSent = false;

        // If backed by a database Partner Organization, synchronize decision with OrganizationService
        if (req.OrganizationId.HasValue && _scopeFactory != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var orgService = scope.ServiceProvider.GetService<IOrganizationService>();
            if (orgService != null)
            {
                var decisionCode = normalizedStatus switch
                {
                    "Approved" => "Approve",
                    "Rejected" => "Reject",
                    "Additional Info" => "RequestInfo",
                    _ => "Approve"
                };

                var reviewResult = await orgService.ReviewOrganizationAsync(
                    req.OrganizationId.Value,
                    new OrganizationReviewDecisionRequestDto
                    {
                        Decision = decisionCode,
                        Rationale = rationale
                    },
                    reviewer);

                if (!reviewResult.Success)
                {
                    return ApiResponse<VerificationRequestDto>.FailureResult(reviewResult.Message);
                }

                emailSent = reviewResult.Data?.EmailSent ?? false;
            }
        }
        else
        {
            try
            {
                var actionName = normalizedStatus switch
                {
                    "Approved" => "Verification approved",
                    "Rejected" => "Verification rejected",
                    "Additional Info" => "Verification additional info requested",
                    _ => $"Verification status set to {normalizedStatus}"
                };

                await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
                {
                    Action = actionName,
                    Actor = reviewer,
                    Record = req.Id,
                    Change = $"{previousStatus} → {normalizedStatus}",
                    Rationale = rationale,
                    Type = req.Type.Contains("Organisation", StringComparison.OrdinalIgnoreCase) ? "Organisation" : "User",
                    Environment = "Production"
                });
            }
            catch
            {
                // Non-fatal
            }
        }

        req.Status = normalizedStatus;
        req.DecisionRationale = rationale;
        if (normalizedStatus == "Additional Info")
        {
            req.AdditionalInfoRequest = rationale;
        }
        req.ReviewedBy = reviewer;
        req.ReviewedAt = DateTime.UtcNow;

        var dto = MapToDto(req);
        dto.EmailSent = emailSent;

        return ApiResponse<VerificationRequestDto>.SuccessResult(
            dto,
            $"Verification decision '{normalizedStatus}' recorded successfully for request {req.Id}.");
    }

    private static string NormalizeVerificationStatus(string status)
    {
        if (status.Equals("Approve", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            return "Approved";
        }

        if (status.Equals("Reject", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return "Rejected";
        }

        if (status.Equals("RequestInfo", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("Additional Info", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("AdditionalInfoRequested", StringComparison.OrdinalIgnoreCase))
        {
            return "Additional Info";
        }

        return status;
    }

    public async Task<byte[]> ExportCsvAsync(VerificationQueryDto query)
    {
        await SyncDatabaseOrganizationsAsync();

        var items = FilterRequests(query).OrderByDescending(r => r.SubmittedAt).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("Request ID,Applicant,Request Type,Evidence,Priority,Submitted,Status,Decision Rationale,Reviewed By");

        foreach (var r in items)
        {
            var row = string.Join(",",
                EscapeCsv(r.Id),
                EscapeCsv(r.Applicant),
                EscapeCsv(r.Type),
                EscapeCsv(r.Evidence),
                EscapeCsv(r.Priority),
                EscapeCsv(FormatSubmitted(r.SubmittedAt)),
                EscapeCsv(r.Status),
                EscapeCsv(r.DecisionRationale ?? string.Empty),
                EscapeCsv(r.ReviewedBy ?? string.Empty)
            );
            sb.AppendLine(row);
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private IEnumerable<VerificationRequest> FilterRequests(VerificationQueryDto query)
    {
        var items = _requests.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var q = query.Search.Trim().ToLowerInvariant();
            items = items.Where(r =>
                r.Id.ToLowerInvariant().Contains(q) ||
                r.Applicant.ToLowerInvariant().Contains(q) ||
                r.Type.ToLowerInvariant().Contains(q) ||
                r.Evidence.ToLowerInvariant().Contains(q) ||
                (r.OrganizationName != null && r.OrganizationName.ToLowerInvariant().Contains(q)) ||
                (r.RepresentativeFullName != null && r.RepresentativeFullName.ToLowerInvariant().Contains(q)) ||
                (r.ApplicantEmail != null && r.ApplicantEmail.ToLowerInvariant().Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(query.Type) &&
            !query.Type.Equals("All request types", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(r => r.Type.Equals(query.Type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Priority) &&
            !query.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(r => r.Priority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            !query.Status.Equals("All states", StringComparison.OrdinalIgnoreCase) &&
            !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(r => r.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.DateFilter) &&
            !query.DateFilter.Equals("Date submitted", StringComparison.OrdinalIgnoreCase) &&
            !query.DateFilter.Equals("All time", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.UtcNow;
            if (query.DateFilter.Equals("Today", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(r => r.SubmittedAt >= now.Date);
            }
            else if (query.DateFilter.Equals("Older", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(r => r.SubmittedAt < now.Date);
            }
        }

        return items;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return $"\"{value}\"";
    }

    private static VerificationRequestDto MapToDto(VerificationRequest r)
    {
        return new VerificationRequestDto
        {
            Id = r.Id,
            OrganizationId = r.OrganizationId,
            UserId = r.UserId,
            Applicant = r.Applicant,
            Type = r.Type,
            Evidence = r.Evidence,
            Priority = r.Priority,
            Submitted = FormatSubmitted(r.SubmittedAt),
            SubmittedAt = r.SubmittedAt,
            Status = r.Status,
            ApplicantEmail = r.ApplicantEmail,
            ApplicantPhone = r.ApplicantPhone,
            OrganizationName = r.OrganizationName,
            OrganizationType = r.OrganizationType,
            OtherOrganizationType = r.OtherOrganizationType,
            RegistrationCountry = r.RegistrationCountry,
            RegistrationNumber = r.RegistrationNumber,
            YearEstablished = r.YearEstablished,
            Website = r.Website,
            SecondaryContactPhone = r.SecondaryContactPhone,
            Address = r.Address,
            District = r.District,
            Province = r.Province,
            Description = r.Description,
            AreasOfOperation = SplitCsvList(r.AreasOfOperation),
            AreasOfAssistance = SplitCsvList(r.AreasOfAssistance),
            OtherAreaOfAssistance = r.OtherAreaOfAssistance,
            OperationalCapacity = r.OperationalCapacity,
            EmergencyResponseAvailability = r.EmergencyResponseAvailability,
            AdditionalResources = r.AdditionalResources,
            RepresentativeFullName = r.RepresentativeFullName,
            RepresentativeDesignation = r.RepresentativeDesignation,
            RepresentativeEmail = r.RepresentativeEmail,
            RepresentativePhone = r.RepresentativePhone,
            RepresentativeAlternativePhone = r.RepresentativeAlternativePhone,
            LoginEmail = r.LoginEmail,
            ApprovalStatus = r.Status,
            AdditionalInfoRequest = r.AdditionalInfoRequest,
            AdditionalInfoResponse = r.AdditionalInfoResponse,
            ReviewedBy = r.ReviewedBy,
            ReviewedAt = r.ReviewedAt,
            DecisionRationale = r.DecisionRationale,
            Documents = r.Documents.Select(d => new VerificationDocumentDto
            {
                Id = d.Id,
                DocumentId = d.DocumentId,
                OrganizationId = d.OrganizationId,
                Category = d.Category,
                Name = d.Name,
                OriginalFileName = string.IsNullOrWhiteSpace(d.OriginalFileName) ? d.Name : d.OriginalFileName,
                Type = d.Type,
                ContentType = d.ContentType,
                Size = d.Size,
                FileSizeBytes = d.FileSizeBytes,
                Status = d.Status,
                CanDownload = d.CanDownload,
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

    private static string FormatSubmitted(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalHours < 1)
        {
            var minutes = Math.Max(1, (int)diff.TotalMinutes);
            return $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
        }
        if (diff.TotalHours < 24)
        {
            var hours = (int)diff.TotalHours;
            return $"{hours} hour{(hours == 1 ? "" : "s")} ago";
        }
        if (diff.TotalDays < 2)
        {
            return "Yesterday";
        }
        var days = (int)diff.TotalDays;
        return $"{days} days ago";
    }
}
