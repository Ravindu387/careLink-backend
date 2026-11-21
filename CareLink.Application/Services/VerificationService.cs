using System.Collections.Concurrent;
using System.Text;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.DTOs.Verification;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

namespace CareLink.Application.Services;

public class VerificationService : IVerificationService
{
    private readonly ConcurrentDictionary<string, VerificationRequest> _requests = new(StringComparer.OrdinalIgnoreCase);
    private readonly IAuditLogService _auditLogService;
    private int _idSequence = 2050;

    public VerificationService(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
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
                Evidence = "Registration · Tax · Safeguarding",
                Priority = "High",
                Status = "Pending Review",
                SubmittedAt = now.AddHours(-2),
                ApplicantEmail = "partnerships@globalrelief.org",
                ApplicantPhone = "+94112345678",
                OrganizationName = "Global Relief Network",
                Documents =
                [
                    new() { Id = "DOC-101", Name = "Official Registration Certificate", Type = "PDF", Size = "2.4 MB", Status = "Verified", UploadedAt = now.AddHours(-2) },
                    new() { Id = "DOC-102", Name = "Tax Compliance Declaration", Type = "PDF", Size = "1.1 MB", Status = "Ready", UploadedAt = now.AddHours(-2) },
                    new() { Id = "DOC-103", Name = "Child Safeguarding & Protection Policy", Type = "PDF", Size = "3.8 MB", Status = "Ready", UploadedAt = now.AddHours(-2) }
                ]
            },
            new()
            {
                Id = "VER-2044",
                Applicant = "Sara Khan",
                Type = "Partner Officer",
                Evidence = "Identity · Employment Letter",
                Priority = "High",
                Status = "Pending Review",
                SubmittedAt = now.AddHours(-5),
                ApplicantEmail = "sara.khan@redcross.lk",
                ApplicantPhone = "+94771234567",
                OrganizationName = "Sri Lanka Red Cross Society",
                Documents =
                [
                    new() { Id = "DOC-104", Name = "National Identity Card (NIC)", Type = "PDF", Size = "1.5 MB", Status = "Ready", UploadedAt = now.AddHours(-5) },
                    new() { Id = "DOC-105", Name = "Official Employment & Authorization Letter", Type = "PDF", Size = "840 KB", Status = "Ready", UploadedAt = now.AddHours(-5) }
                ]
            },
            new()
            {
                Id = "VER-2039",
                Applicant = "Riverton Medical Collective",
                Type = "Medical Partner",
                Evidence = "Registration · Medical Licence",
                Priority = "Standard",
                Status = "Additional Info",
                SubmittedAt = now.AddDays(-1).AddHours(-2),
                ApplicantEmail = "contact@rivertonmedical.org",
                ApplicantPhone = "+94114567890",
                OrganizationName = "Riverton Medical Collective",
                DecisionRationale = "Medical license document was partially blurred. Requested renewed certified copy.",
                ReviewedBy = "Alex Kimani",
                ReviewedAt = now.AddHours(-4),
                Documents =
                [
                    new() { Id = "DOC-106", Name = "Hospital Clinical Registration", Type = "PDF", Size = "3.2 MB", Status = "Verified", UploadedAt = now.AddDays(-1) },
                    new() { Id = "DOC-107", Name = "Practitioner Medical Council Licence", Type = "PDF", Size = "1.8 MB", Status = "Under Review", UploadedAt = now.AddDays(-1) }
                ]
            },
            new()
            {
                Id = "VER-2031",
                Applicant = "Abdul Rahman",
                Type = "Resource Manager",
                Evidence = "Identity · Appointment Letter",
                Priority = "Standard",
                Status = "Pending Review",
                SubmittedAt = now.AddDays(-1).AddHours(-6),
                ApplicantEmail = "abdul.rahman@dmc.gov.lk",
                ApplicantPhone = "+94779876543",
                OrganizationName = "National Disaster Management Centre",
                Documents =
                [
                    new() { Id = "DOC-108", Name = "Government Issued NIC", Type = "PDF", Size = "1.2 MB", Status = "Ready", UploadedAt = now.AddDays(-1) },
                    new() { Id = "DOC-109", Name = "Divisional Appointment Letter", Type = "PDF", Size = "920 KB", Status = "Ready", UploadedAt = now.AddDays(-1) }
                ]
            },
            new()
            {
                Id = "VER-2027",
                Applicant = "Community Response Alliance",
                Type = "Partner Organisation",
                Evidence = "Registration · Tax",
                Priority = "High",
                Status = "Pending Review",
                SubmittedAt = now.AddDays(-2),
                ApplicantEmail = "info@cra-relief.org",
                ApplicantPhone = "+94117654321",
                OrganizationName = "Community Response Alliance",
                Documents =
                [
                    new() { Id = "DOC-110", Name = "NGO Secretariat Registration Certificate", Type = "PDF", Size = "2.8 MB", Status = "Ready", UploadedAt = now.AddDays(-2) },
                    new() { Id = "DOC-111", Name = "Annual Tax Clearance Proof", Type = "PDF", Size = "1.4 MB", Status = "Ready", UploadedAt = now.AddDays(-2) }
                ]
            },
            new()
            {
                Id = "VER-2022",
                Applicant = "Daniel Perera",
                Type = "Resource Manager",
                Evidence = "Identity · Employment Letter",
                Priority = "Standard",
                Status = "Approved",
                SubmittedAt = now.AddDays(-3),
                ReviewedAt = now.AddHours(-1),
                ReviewedBy = "System Administrator",
                DecisionRationale = "All government employment credentials and NIC records verified successfully.",
                ApplicantEmail = "daniel.perera@carelink.org",
                ApplicantPhone = "+94774561234",
                OrganizationName = "CareLink Logistics Network",
                Documents =
                [
                    new() { Id = "DOC-112", Name = "Passport / NIC Scan", Type = "PDF", Size = "1.9 MB", Status = "Verified", UploadedAt = now.AddDays(-3) },
                    new() { Id = "DOC-113", Name = "Verified Letter of Employment", Type = "PDF", Size = "780 KB", Status = "Verified", UploadedAt = now.AddDays(-3) }
                ]
            },
            new()
            {
                Id = "VER-2018",
                Applicant = "Emergency Logistics Unit",
                Type = "Partner Organisation",
                Evidence = "Registration · Tax · Safeguarding",
                Priority = "High",
                Status = "Pending Review",
                SubmittedAt = now.AddDays(-4),
                ApplicantEmail = "ops@emergencylogistics.lk",
                ApplicantPhone = "+94118765432",
                OrganizationName = "Emergency Logistics Unit",
                Documents =
                [
                    new() { Id = "DOC-114", Name = "Corporate Registration Incorporation", Type = "PDF", Size = "2.2 MB", Status = "Ready", UploadedAt = now.AddDays(-4) },
                    new() { Id = "DOC-115", Name = "Tax Compliance Docket", Type = "PDF", Size = "1.3 MB", Status = "Ready", UploadedAt = now.AddDays(-4) },
                    new() { Id = "DOC-116", Name = "Aid Worker Code of Conduct", Type = "PDF", Size = "3.1 MB", Status = "Ready", UploadedAt = now.AddDays(-4) }
                ]
            },
            new()
            {
                Id = "VER-2012",
                Applicant = "Nadia Okafor",
                Type = "Partner Officer",
                Evidence = "Identity · Appointment Letter",
                Priority = "Standard",
                Status = "Rejected",
                SubmittedAt = now.AddDays(-5),
                ReviewedAt = now.AddDays(-4),
                ReviewedBy = "Marta Silva",
                DecisionRationale = "Unable to authenticate organizational affiliation. Phone and email domain verification failed.",
                ApplicantEmail = "nadia.okafor@freemail.com",
                ApplicantPhone = "+94776549870",
                OrganizationName = "Independent Field Volunteer",
                Documents =
                [
                    new() { Id = "DOC-117", Name = "Photocopy Identity Card", Type = "PDF", Size = "950 KB", Status = "Ready", UploadedAt = now.AddDays(-5) },
                    new() { Id = "DOC-118", Name = "Unofficial Recommendation Note", Type = "PDF", Size = "420 KB", Status = "Ready", UploadedAt = now.AddDays(-5) }
                ]
            },
            new()
            {
                Id = "VER-2007",
                Applicant = "Highland Relief Volunteers",
                Type = "Partner Organisation",
                Evidence = "Registration · Volunteer Charter",
                Priority = "Standard",
                Status = "Approved",
                SubmittedAt = now.AddDays(-6),
                ReviewedAt = now.AddHours(-2),
                ReviewedBy = "Alex Kimani",
                DecisionRationale = "Volunteer NGO registration confirmed with provincial secretary.",
                ApplicantEmail = "highland@volunteers.org",
                ApplicantPhone = "+94812345678",
                OrganizationName = "Highland Relief Volunteers",
                Documents =
                [
                    new() { Id = "DOC-119", Name = "Provincial Registration", Type = "PDF", Size = "1.6 MB", Status = "Verified", UploadedAt = now.AddDays(-6) }
                ]
            }
        };

        foreach (var req in seedList)
        {
            _requests[req.Id] = req;
        }
    }

    public Task<ApiResponse<PaginatedListDto<VerificationRequestDto>>> GetRequestsAsync(VerificationQueryDto query)
    {
        var items = FilterRequests(query);
        var ordered = items.OrderByDescending(r => r.SubmittedAt).ToList();

        var totalCount = ordered.Count;
        var page = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

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

        return Task.FromResult(ApiResponse<PaginatedListDto<VerificationRequestDto>>.SuccessResult(result));
    }

    public Task<ApiResponse<VerificationRequestDto>> GetRequestByIdAsync(string id)
    {
        if (_requests.TryGetValue(id, out var req))
        {
            return Task.FromResult(ApiResponse<VerificationRequestDto>.SuccessResult(MapToDto(req)));
        }

        return Task.FromResult(ApiResponse<VerificationRequestDto>.FailureResult($"Verification request '{id}' not found."));
    }

    public Task<ApiResponse<VerificationStatsDto>> GetStatsAsync()
    {
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
                oldestPendingText = $"Oldest unresolved request was submitted {(int)diff.TotalHours} hours ago.";
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

        return Task.FromResult(ApiResponse<VerificationStatsDto>.SuccessResult(stats));
    }

    public Task<ApiResponse<IReadOnlyList<string>>> GetRequestTypesAsync()
    {
        var types = _requests.Values
            .Select(r => r.Type)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        return Task.FromResult(ApiResponse<IReadOnlyList<string>>.SuccessResult(types));
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
                    Type = "PDF",
                    Size = "1.5 MB",
                    Status = "Ready",
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
                Type = "PDF",
                Size = "1.2 MB",
                Status = "Ready",
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

    public async Task<ApiResponse<VerificationRequestDto>> RecordDecisionAsync(string id, VerificationDecisionRequestDto request, string actor)
    {
        if (!_requests.TryGetValue(id, out var req))
        {
            return ApiResponse<VerificationRequestDto>.FailureResult($"Verification request '{id}' not found.");
        }

        var previousStatus = req.Status;
        req.Status = request.Status.Trim();
        req.DecisionRationale = request.Rationale.Trim();
        req.ReviewedBy = string.IsNullOrWhiteSpace(actor) ? "System Administrator" : actor;
        req.ReviewedAt = DateTime.UtcNow;

        if (req.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var doc in req.Documents)
            {
                doc.Status = "Verified";
            }
        }
        else if (req.Status.Equals("Additional Info", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var doc in req.Documents)
            {
                if (doc.Status != "Verified") doc.Status = "Under Review";
            }
        }

        // Write an immutable entry into AuditLog
        try
        {
            var actionName = req.Status switch
            {
                "Approved" => "Verification approved",
                "Rejected" => "Verification rejected",
                "Additional Info" => "Verification additional info requested",
                _ => $"Verification status set to {req.Status}"
            };

            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
            {
                Action = actionName,
                Actor = req.ReviewedBy,
                Record = req.Id,
                Change = $"{previousStatus} → {req.Status}",
                Rationale = req.DecisionRationale,
                Type = req.Type.Contains("Organisation", StringComparison.OrdinalIgnoreCase) ? "Organisation" : "User",
                Environment = "Production"
            });
        }
        catch
        {
            // Logging failure should not disrupt the transaction
        }

        return ApiResponse<VerificationRequestDto>.SuccessResult(
            MapToDto(req), $"Verification decision '{req.Status}' recorded successfully for request {req.Id}.");
    }

    public Task<byte[]> ExportCsvAsync(VerificationQueryDto query)
    {
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

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
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
                (r.OrganizationName != null && r.OrganizationName.ToLowerInvariant().Contains(q)));
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
            ReviewedBy = r.ReviewedBy,
            ReviewedAt = r.ReviewedAt,
            DecisionRationale = r.DecisionRationale,
            Documents = r.Documents.Select(d => new VerificationDocumentDto
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type,
                Size = d.Size,
                Status = d.Status,
                UploadedAt = d.UploadedAt
            }).ToList()
        };
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
