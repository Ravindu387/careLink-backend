using System.Collections.Concurrent;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AssistanceRequests;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CareLink.Application.Services;

public class AssistanceRequestService : IAssistanceRequestService
{
    private readonly ConcurrentDictionary<string, AssistanceRequestDto> _requests = new(StringComparer.OrdinalIgnoreCase);
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AssistanceRequestService> _logger;
    private int _idSequence = 2850;

    public AssistanceRequestService(
        IAuditLogService auditLogService,
        ILogger<AssistanceRequestService> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
        InitializeDefaultRequests();
    }

    private void InitializeDefaultRequests()
    {
        var now = DateTime.UtcNow;

        var seed = new List<AssistanceRequestDto>
        {
            new()
            {
                Id = "REQ-2841",
                CreatedAt = now.AddMinutes(-12),
                TimeAgo = "12 min ago",
                Category = "Emergency shelter",
                Location = "Riverside, North Ward",
                ReportedPriority = "Critical",
                Recommendation = "Critical priority",
                Confidence = "94%",
                AssignedTo = "Unassigned",
                Status = "Awaiting review",
                AffectedPeople = 6,
                Contact = "Verified · ending 0198",
                Description = "Water has reached our ground floor and is rising. We have two young children and my mother cannot walk without assistance.",
                CrisisName = "Monsoon Floods 2025"
            },
            new()
            {
                Id = "REQ-2839",
                CreatedAt = now.AddMinutes(-26),
                TimeAgo = "26 min ago",
                Category = "Medical supplies",
                Location = "Old Town, Sector 4",
                ReportedPriority = "High",
                Recommendation = "High priority",
                Confidence = "91%",
                AssignedTo = "Medical Team 3",
                Status = "Verified",
                AffectedPeople = 3,
                Contact = "Verified · ending 0284",
                Description = "Urgent medical supplies needed for individuals isolated by flood waters in Sector 4.",
                CrisisName = "Monsoon Floods 2025"
            },
            new()
            {
                Id = "REQ-2836",
                CreatedAt = now.AddMinutes(-41),
                TimeAgo = "41 min ago",
                Category = "Food & water",
                Location = "Eastbank, Sector 2",
                ReportedPriority = "High",
                Recommendation = "High priority",
                Confidence = "87%",
                AssignedTo = "Relief Squad B",
                Status = "Assigned",
                AffectedPeople = 3,
                Contact = "Verified · ending 0891",
                Description = "Household of 3 isolated without clean drinking water and shelf-stable rations since yesterday.",
                CrisisName = "Monsoon Floods 2025"
            },
            new()
            {
                Id = "REQ-2832",
                CreatedAt = now.AddHours(-1).AddMinutes(-15),
                TimeAgo = "1 hr ago",
                Category = "Evacuation support",
                Location = "West Ward, Sector 1",
                ReportedPriority = "Critical",
                Recommendation = "Critical priority",
                Confidence = "96%",
                AssignedTo = "Disaster Boat Unit 2",
                Status = "In progress",
                AffectedPeople = 8,
                Contact = "Verified · ending 0442",
                Description = "Elderly care facility cutoff by road washaway. Immediate boat transfer required.",
                CrisisName = "Monsoon Floods 2025"
            },
            new()
            {
                Id = "REQ-2828",
                CreatedAt = now.AddHours(-2),
                TimeAgo = "2 hrs ago",
                Category = "Emergency shelter",
                Location = "South District, Block C",
                ReportedPriority = "Medium",
                Recommendation = "Medium priority",
                Confidence = "82%",
                AssignedTo = "Shelter Unit A",
                Status = "Completed",
                AffectedPeople = 4,
                Contact = "Verified · ending 0773",
                Description = "Family evacuated from low-lying river perimeter and successfully accommodated at Central School Shelter.",
                CrisisName = "Monsoon Floods 2025"
            },
            new()
            {
                Id = "REQ-2821",
                CreatedAt = now.AddHours(-3),
                TimeAgo = "3 hrs ago",
                Category = "Food & water",
                Location = "Highland Camp",
                ReportedPriority = "Medium",
                Recommendation = "Medium priority",
                Confidence = "79%",
                AssignedTo = "Community Logistics",
                Status = "Completed",
                AffectedPeople = 12,
                Contact = "Verified · ending 0519",
                Description = "120 rations and water purification packs delivered to temporary relief cluster.",
                CrisisName = "Monsoon Floods 2025"
            }
        };

        foreach (var req in seed)
        {
            _requests[req.Id] = req;
        }
    }

    public Task<ApiResponse<PaginatedListDto<AssistanceRequestDto>>> GetRequestsAsync(AssistanceRequestQueryDto query)
    {
        var filtered = _requests.Values.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            filtered = filtered.Where(r =>
                r.Id.ToLower().Contains(s) ||
                r.Description.ToLower().Contains(s) ||
                r.Location.ToLower().Contains(s) ||
                r.Category.ToLower().Contains(s) ||
                r.AssignedTo.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All", StringComparison.OrdinalIgnoreCase) && !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(r => r.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Priority) && !query.Priority.Equals("All", StringComparison.OrdinalIgnoreCase) && !query.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(r => r.ReportedPriority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Category) && !query.Category.Equals("All", StringComparison.OrdinalIgnoreCase) && !query.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(r => r.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
        }

        var total = filtered.Count();
        var page = Math.Max(1, query.PageNumber);
        var size = Math.Max(1, Math.Min(100, query.PageSize));

        var items = filtered
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        var result = new PaginatedListDto<AssistanceRequestDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = size
        };
        return Task.FromResult(ApiResponse<PaginatedListDto<AssistanceRequestDto>>.SuccessResult(result));
    }

    public Task<ApiResponse<AssistanceRequestStatsDto>> GetStatsAsync()
    {
        var all = _requests.Values.ToList();
        var stats = new AssistanceRequestStatsDto
        {
            TotalRequests = all.Count,
            AwaitingReviewCount = all.Count(r => r.Status.Equals("Awaiting review", StringComparison.OrdinalIgnoreCase)),
            VerifiedCount = all.Count(r => r.Status.Equals("Verified", StringComparison.OrdinalIgnoreCase)),
            AssignedCount = all.Count(r => r.Status.Equals("Assigned", StringComparison.OrdinalIgnoreCase)),
            InProgressCount = all.Count(r => r.Status.Equals("In progress", StringComparison.OrdinalIgnoreCase)),
            CompletedCount = all.Count(r => r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            CriticalCount = all.Count(r => r.ReportedPriority.Equals("Critical", StringComparison.OrdinalIgnoreCase))
        };

        return Task.FromResult(ApiResponse<AssistanceRequestStatsDto>.SuccessResult(stats));
    }

    public Task<ApiResponse<AssistanceRequestDto>> GetByIdAsync(string id)
    {
        if (_requests.TryGetValue(id, out var req))
        {
            return Task.FromResult(ApiResponse<AssistanceRequestDto>.SuccessResult(req));
        }
        return Task.FromResult(ApiResponse<AssistanceRequestDto>.FailureResult("Assistance request not found."));
    }

    public Task<ApiResponse<AssistanceRequestDto>> CreateRequestAsync(CreateAssistanceRequestDto request, string actor)
    {
        var id = $"REQ-{Interlocked.Increment(ref _idSequence)}";
        var newReq = new AssistanceRequestDto
        {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            TimeAgo = "Just now",
            Category = request.Category,
            Location = request.Location,
            ReportedPriority = request.ReportedPriority,
            Recommendation = $"{request.ReportedPriority} priority",
            Confidence = "90%",
            AssignedTo = "Unassigned",
            Status = "Awaiting review",
            AffectedPeople = request.AffectedPeople,
            Contact = request.Contact,
            Description = request.Description,
            CrisisName = request.CrisisName ?? "Monsoon Floods 2025"
        };

        _requests[id] = newReq;
        _logger.LogInformation("Assistance request {RequestId} logged by {Actor}", id, actor);

        return Task.FromResult(ApiResponse<AssistanceRequestDto>.SuccessResult(newReq, "Assistance request logged successfully."));
    }

    public Task<ApiResponse<AssistanceRequestDto>> UpdateRequestAsync(string id, UpdateAssistanceRequestDto request, string actor)
    {
        if (!_requests.TryGetValue(id, out var req))
            return Task.FromResult(ApiResponse<AssistanceRequestDto>.FailureResult("Request not found."));

        req.Category = request.Category;
        req.Location = request.Location;
        req.ReportedPriority = request.ReportedPriority;
        req.AffectedPeople = request.AffectedPeople;
        req.Contact = request.Contact;
        req.Description = request.Description;

        return Task.FromResult(ApiResponse<AssistanceRequestDto>.SuccessResult(req, "Request updated."));
    }

    public Task<ApiResponse<AssistanceRequestDto>> UpdateStatusAsync(string id, UpdateRequestStatusDto request, string actor)
    {
        if (!_requests.TryGetValue(id, out var req))
            return Task.FromResult(ApiResponse<AssistanceRequestDto>.FailureResult("Request not found."));

        var oldStatus = req.Status;
        req.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Reason))
            req.RejectionReason = request.Reason;
        if (!string.IsNullOrWhiteSpace(request.Note))
            req.FieldVerificationNote = request.Note;

        _logger.LogInformation("Assistance request {RequestId} status changed from {Old} to {New} by {Actor}", id, oldStatus, request.Status, actor);
        return Task.FromResult(ApiResponse<AssistanceRequestDto>.SuccessResult(req, $"Status updated to {request.Status}."));
    }

    public Task<ApiResponse<AssistanceRequestDto>> UpdatePriorityAsync(string id, UpdateRequestPriorityDto request, string actor)
    {
        if (!_requests.TryGetValue(id, out var req))
            return Task.FromResult(ApiResponse<AssistanceRequestDto>.FailureResult("Request not found."));

        req.ReportedPriority = request.Priority;
        _logger.LogInformation("Assistance request {RequestId} priority updated to {Priority} by {Actor}", id, request.Priority, actor);
        return Task.FromResult(ApiResponse<AssistanceRequestDto>.SuccessResult(req, $"Priority updated to {request.Priority}."));
    }

    public Task<ApiResponse<AssistanceRequestDto>> AssignRequestAsync(string id, AssignRequestDto request, string actor)
    {
        if (!_requests.TryGetValue(id, out var req))
            return Task.FromResult(ApiResponse<AssistanceRequestDto>.FailureResult("Request not found."));

        req.AssignedTo = request.AssignedTo;
        if (req.Status == "Awaiting review" || req.Status == "Verified")
            req.Status = "Assigned";

        _logger.LogInformation("Assistance request {RequestId} assigned to {Team} by {Actor}", id, request.AssignedTo, actor);
        return Task.FromResult(ApiResponse<AssistanceRequestDto>.SuccessResult(req, $"Assigned to {request.AssignedTo}."));
    }

    public Task<ApiResponse<bool>> DeleteRequestAsync(string id, string actor)
    {
        var removed = _requests.TryRemove(id, out _);
        if (!removed)
            return Task.FromResult(ApiResponse<bool>.FailureResult("Request not found."));

        _logger.LogInformation("Assistance request {RequestId} deleted by {Actor}", id, actor);
        return Task.FromResult(ApiResponse<bool>.SuccessResult(true, "Request removed."));
    }
}
