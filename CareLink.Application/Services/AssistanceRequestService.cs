using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using Microsoft.Extensions.Logging;
using AdminDtos = CareLink.Application.DTOs.AssistanceRequests;

namespace CareLink.Application.Services;

public class AssistanceRequestService : IAssistanceRequestService
{
    private readonly IAssistanceRequestRepository _repository;
    private readonly IAIInferenceEngine _aiEngine;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AssistanceRequestService> _logger;
    private static int _requestCounter = 2855;
    private static readonly object _counterLock = new();

    public AssistanceRequestService(
        IAssistanceRequestRepository repository,
        IAIInferenceEngine aiEngine,
        IAuditLogService auditLogService,
        ILogger<AssistanceRequestService> logger)
    {
        _repository = repository;
        _aiEngine = aiEngine;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    #region Operational & Coordinator Methods (Safna)

    public async Task<IReadOnlyList<AssistanceRequestResponseDto>> GetAllAsync(
        AssistanceRequestQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var filterParams = new AssistanceRequestFilterParams
        {
            Search = query.Search,
            Tab = query.Tab,
            CrisisEvent = query.CrisisEvent,
            Priority = query.Priority,
            Category = query.Category,
            Status = query.Status,
            Organisation = query.Organisation,
            VerifiedOnly = query.VerifiedOnly,
            UnassignedOnly = query.UnassignedOnly
        };

        var list = await _repository.GetAllAsync(filterParams, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<AssistanceRequestResponseDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdAsync(id, cancellationToken);
        return request == null ? null : MapToDto(request);
    }

    public async Task<AssistanceRequestResponseDto> CreateAsync(
        CareLink.Application.DTOs.CreateAssistanceRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        string id;
        lock (_counterLock)
        {
            _requestCounter++;
            id = $"REQ-{_requestCounter}";
        }

        string priorityLevel;
        int confidence;
        string priorityDisplay;

        if (!string.IsNullOrWhiteSpace(dto.Priority) && dto.Priority.Contains('·'))
        {
            var parts = dto.Priority.Split('·', StringSplitOptions.TrimEntries);
            priorityLevel = parts[0];
            int.TryParse(parts[1].Replace("%", "").Trim(), out confidence);
            if (confidence == 0) confidence = 85;
            priorityDisplay = $"{priorityLevel} · {confidence}%";
        }
        else
        {
            // Evaluate with AI engine
            var rec = _aiEngine.InferRecommendation(new GenerateRecommendationRequestDto
            {
                RequestId = id,
                Category = dto.Category,
                Location = dto.Location,
                Description = dto.Description,
                AffectedPeopleCount = dto.AffectedPeopleCount,
                HasVulnerableMembers = dto.HasVulnerableMembers,
                IsLifeThreatening = dto.IsLifeThreatening,
                PriorityHint = dto.Priority
            });

            priorityLevel = rec.Priority;
            confidence = rec.Confidence;
            priorityDisplay = $"{priorityLevel} · {confidence}%";
        }

        var isLifeThreatening = dto.IsLifeThreatening || priorityLevel.Equals("Critical", StringComparison.OrdinalIgnoreCase);

        var entity = new AssistanceRequest
        {
            Id = id,
            Category = dto.Category.Trim(),
            Location = dto.Location.Trim(),
            CrisisEvent = string.IsNullOrWhiteSpace(dto.CrisisEvent) ? "Monsoon Floods 2025" : dto.CrisisEvent.Trim(),
            PriorityLevel = priorityLevel,
            Confidence = confidence,
            PriorityDisplay = priorityDisplay,
            Verification = string.IsNullOrWhiteSpace(dto.Verification) ? "Pending verification" : dto.Verification.Trim(),
            Assigned = string.IsNullOrWhiteSpace(dto.Assigned) ? "Unassigned" : dto.Assigned.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Submitted" : dto.Status.Trim(),
            Organisation = string.IsNullOrWhiteSpace(dto.Organisation) ? "National Disaster Authority" : dto.Organisation.Trim(),
            Description = dto.Description,
            AffectedPeopleCount = dto.AffectedPeopleCount ?? 1,
            HasVulnerableMembers = dto.HasVulnerableMembers,
            IsLifeThreatening = isLifeThreatening,
            IsOverdue = false,
            IsPotentialDuplicate = false,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<AssistanceRequestResponseDto?> UpdateAsync(
        string id,
        CareLink.Application.DTOs.UpdateAssistanceRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Category)) existing.Category = dto.Category.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Location)) existing.Location = dto.Location.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Priority))
        {
            existing.PriorityDisplay = dto.Priority.Trim();
            if (dto.Priority.Contains('·'))
            {
                var parts = dto.Priority.Split('·', StringSplitOptions.TrimEntries);
                existing.PriorityLevel = parts[0];
                if (int.TryParse(parts[1].Replace("%", "").Trim(), out var conf))
                {
                    existing.Confidence = conf;
                }
            }
            else
            {
                existing.PriorityLevel = dto.Priority.Trim();
            }
        }
        if (!string.IsNullOrWhiteSpace(dto.Verification)) existing.Verification = dto.Verification.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Assigned))
        {
            existing.Assigned = dto.Assigned.Trim();
            if (!existing.Assigned.Equals("Unassigned", StringComparison.OrdinalIgnoreCase))
            {
                existing.AssignedAt = DateTimeOffset.UtcNow;
                if (existing.Status.Equals("Submitted", StringComparison.OrdinalIgnoreCase))
                {
                    existing.Status = "Assigned";
                }
            }
        }
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Organisation)) existing.Organisation = dto.Organisation.Trim();
        if (dto.Description != null) existing.Description = dto.Description;
        if (dto.AffectedPeopleCount.HasValue) existing.AffectedPeopleCount = dto.AffectedPeopleCount.Value;
        if (dto.HasVulnerableMembers.HasValue) existing.HasVulnerableMembers = dto.HasVulnerableMembers.Value;
        if (dto.IsLifeThreatening.HasValue) existing.IsLifeThreatening = dto.IsLifeThreatening.Value;

        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<AssistanceRequestResponseDto?> AssignAsync(
        string id,
        AssignAssistanceRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Assigned = dto.Assigned.Trim();
        existing.AssignedAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Organisation))
        {
            existing.Organisation = dto.Organisation.Trim();
        }

        if (existing.Status.Equals("Submitted", StringComparison.OrdinalIgnoreCase) ||
            existing.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase))
        {
            existing.Status = "Assigned";
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<AssistanceRequestResponseDto?> UpdateStatusAsync(
        string id,
        UpdateAssistanceRequestStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Status = dto.Status.Trim();
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        if (existing.Status.Equals("Escalated", StringComparison.OrdinalIgnoreCase))
        {
            existing.IsOverdue = true;
        }

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<AssistanceRequestMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _repository.GetMetricsAsync(cancellationToken);
        return new AssistanceRequestMetricsDto
        {
            TotalRequests = metrics.TotalRequests,
            Unassigned = metrics.Unassigned,
            Overdue = metrics.Overdue,
            PotentialDuplicates = metrics.PotentialDuplicates,
            UnderReview = metrics.UnderReview,
            InProgress = metrics.InProgress,
            Completed = metrics.Completed,
            Escalated = metrics.Escalated
        };
    }

    public async Task ResetDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    #endregion

    #region Administrative & Management Methods (dev)

    public async Task<ApiResponse<PaginatedListDto<AdminDtos.AssistanceRequestDto>>> GetRequestsAsync(
        AdminDtos.AssistanceRequestQueryDto query)
    {
        var allEntities = await _repository.GetAllAsync();
        var queryable = allEntities.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            queryable = queryable.Where(r =>
                r.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                r.Category.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                r.Location.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                r.Assigned.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                (r.Description != null && r.Description.Contains(s, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            queryable = queryable.Where(r => r.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Priority))
        {
            queryable = queryable.Where(r => r.PriorityLevel.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            queryable = queryable.Where(r => r.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Crisis))
        {
            queryable = queryable.Where(r => r.CrisisEvent.Equals(query.Crisis, StringComparison.OrdinalIgnoreCase));
        }

        var list = queryable.Select(MapToAdminDto).ToList();
        var total = list.Count;
        var paged = list
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var result = new PaginatedListDto<AdminDtos.AssistanceRequestDto>
        {
            Items = paged,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        return ApiResponse<PaginatedListDto<AdminDtos.AssistanceRequestDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<AdminDtos.AssistanceRequestStatsDto>> GetStatsAsync()
    {
        var all = await _repository.GetAllAsync();
        var stats = new AdminDtos.AssistanceRequestStatsDto
        {
            TotalRequests = all.Count,
            AwaitingReviewCount = all.Count(r => r.Status.Equals("Awaiting review", StringComparison.OrdinalIgnoreCase) || r.Status.Equals("Submitted", StringComparison.OrdinalIgnoreCase) || r.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase)),
            VerifiedCount = all.Count(r => r.Verification.Equals("Verified", StringComparison.OrdinalIgnoreCase) || r.Verification.Equals("Contact verified", StringComparison.OrdinalIgnoreCase)),
            AssignedCount = all.Count(r => r.Status.Equals("Assigned", StringComparison.OrdinalIgnoreCase)),
            InProgressCount = all.Count(r => r.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase) || r.Status.Equals("In progress", StringComparison.OrdinalIgnoreCase)),
            CompletedCount = all.Count(r => r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            CriticalCount = all.Count(r => r.PriorityLevel.Equals("Critical", StringComparison.OrdinalIgnoreCase))
        };

        return ApiResponse<AdminDtos.AssistanceRequestStatsDto>.SuccessResult(stats);
    }

    public async Task<ApiResponse<AdminDtos.AssistanceRequestDto>> GetAdminByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            return ApiResponse<AdminDtos.AssistanceRequestDto>.FailureResult($"Assistance request '{id}' not found.");
        }

        return ApiResponse<AdminDtos.AssistanceRequestDto>.SuccessResult(MapToAdminDto(entity));
    }

    public async Task<ApiResponse<AdminDtos.AssistanceRequestDto>> CreateRequestAsync(
        AdminDtos.CreateAssistanceRequestDto request,
        string actor)
    {
        string id;
        lock (_counterLock)
        {
            _requestCounter++;
            id = $"REQ-{_requestCounter}";
        }

        var entity = new AssistanceRequest
        {
            Id = id,
            Category = request.Category.Trim(),
            Location = request.Location.Trim(),
            Description = request.Description.Trim(),
            AffectedPeopleCount = request.AffectedPeople <= 0 ? 1 : request.AffectedPeople,
            CrisisEvent = string.IsNullOrWhiteSpace(request.CrisisName) ? "Monsoon Floods 2025" : request.CrisisName.Trim(),
            PriorityLevel = request.ReportedPriority ?? "High",
            Confidence = 88,
            PriorityDisplay = $"{request.ReportedPriority ?? "High"} Â· 88%",
            Status = "Awaiting review",
            Verification = "Contact verified",
            Assigned = "Unassigned",
            Organisation = "CareLink Emergency Triage",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.AddAsync(entity);
        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
        {
            Action = "CreateRequest",
            Actor = actor,
            Record = created.Id,
            Change = $"Created assistance request for {created.Category} at {created.Location}",
            Rationale = "New request registered",
            Type = "AssistanceRequest"
        });

        return ApiResponse<AdminDtos.AssistanceRequestDto>.SuccessResult(MapToAdminDto(created), "Assistance request submitted.");
    }

    public async Task<ApiResponse<AdminDtos.AssistanceRequestDto>> UpdateRequestAsync(
        string id,
        AdminDtos.UpdateAssistanceRequestDto request,
        string actor)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<AdminDtos.AssistanceRequestDto>.FailureResult($"Request '{id}' not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Category)) existing.Category = request.Category.Trim();
        if (!string.IsNullOrWhiteSpace(request.Location)) existing.Location = request.Location.Trim();
        if (!string.IsNullOrWhiteSpace(request.Description)) existing.Description = request.Description.Trim();
        if (request.AffectedPeople > 0) existing.AffectedPeopleCount = request.AffectedPeople;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
        {
            Action = "UpdateRequest",
            Actor = actor,
            Record = id,
            Change = "Updated request details",
            Rationale = "Request modification requested by user",
            Type = "AssistanceRequest"
        });

        return ApiResponse<AdminDtos.AssistanceRequestDto>.SuccessResult(MapToAdminDto(updated!), "Request updated.");
    }

    public async Task<ApiResponse<AdminDtos.AssistanceRequestDto>> UpdateStatusAsync(
        string id,
        AdminDtos.UpdateRequestStatusDto request,
        string actor)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<AdminDtos.AssistanceRequestDto>.FailureResult($"Request '{id}' not found.");
        }

        existing.Status = request.Status;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
        {
            Action = "UpdateStatus",
            Actor = actor,
            Record = id,
            Change = $"Status changed to {request.Status}",
            Rationale = request.Reason ?? "Status update",
            Type = "AssistanceRequest"
        });

        return ApiResponse<AdminDtos.AssistanceRequestDto>.SuccessResult(MapToAdminDto(updated!), $"Status updated to {request.Status}.");
    }

    public async Task<ApiResponse<AdminDtos.AssistanceRequestDto>> UpdatePriorityAsync(
        string id,
        AdminDtos.UpdateRequestPriorityDto request,
        string actor)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<AdminDtos.AssistanceRequestDto>.FailureResult($"Request '{id}' not found.");
        }

        existing.PriorityLevel = request.Priority;
        existing.PriorityDisplay = $"{request.Priority} Â· {existing.Confidence}%";
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
        {
            Action = "UpdatePriority",
            Actor = actor,
            Record = id,
            Change = $"Priority changed to {request.Priority}",
            Rationale = request.Rationale ?? "Priority update",
            Type = "AssistanceRequest"
        });

        return ApiResponse<AdminDtos.AssistanceRequestDto>.SuccessResult(MapToAdminDto(updated!), $"Priority updated to {request.Priority}.");
    }

    public async Task<ApiResponse<AdminDtos.AssistanceRequestDto>> AssignRequestAsync(
        string id,
        AdminDtos.AssignRequestDto request,
        string actor)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<AdminDtos.AssistanceRequestDto>.FailureResult($"Request '{id}' not found.");
        }

        existing.Assigned = request.AssignedTo.Trim();
        existing.AssignedAt = DateTimeOffset.UtcNow;
        existing.Status = "Assigned";
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
        {
            Action = "AssignRequest",
            Actor = actor,
            Record = id,
            Change = $"Assigned to {request.AssignedTo}",
            Rationale = request.Instructions ?? "Assignment update",
            Type = "AssistanceRequest"
        });

        return ApiResponse<AdminDtos.AssistanceRequestDto>.SuccessResult(MapToAdminDto(updated!), $"Request assigned to {request.AssignedTo}.");
    }

    public async Task<ApiResponse<bool>> DeleteRequestAsync(string id, string actor)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.FailureResult($"Request '{id}' not found.");
        }

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequestDto
        {
            Action = "DeleteRequest",
            Actor = actor,
            Record = id,
            Change = $"Deleted assistance request {id}",
            Rationale = "Deletion requested by authorized user",
            Type = "AssistanceRequest"
        });

        return ApiResponse<bool>.SuccessResult(true, "Request deleted successfully.");
    }

    #endregion

    #region Helper Mappers

    private static AssistanceRequestResponseDto MapToDto(AssistanceRequest entity)
    {
        var now = DateTimeOffset.UtcNow;
        var diff = now - entity.CreatedAt;

        string reported;
        string age;

        if (diff.TotalMinutes < 1)
        {
            reported = "Just now";
            age = "Just now";
        }
        else if (diff.TotalMinutes < 60)
        {
            var mins = Math.Max(1, (int)diff.TotalMinutes);
            reported = $"{mins} min ago";
            age = $"{mins} min";
        }
        else if (diff.TotalHours < 24)
        {
            var hrs = (int)diff.TotalHours;
            var mins = diff.Minutes;
            reported = hrs == 1 ? "1 hr ago" : $"{hrs} hrs ago";
            age = mins > 0 ? $"{hrs}h {mins}m" : $"{hrs}h";
        }
        else
        {
            var days = (int)diff.TotalDays;
            reported = days == 1 ? "1 day ago" : $"{days} days ago";
            age = $"{days}d";
        }

        return new AssistanceRequestResponseDto
        {
            Id = entity.Id,
            Category = entity.Category,
            Location = entity.Location,
            CrisisEvent = entity.CrisisEvent,
            Reported = reported,
            Priority = string.IsNullOrEmpty(entity.PriorityDisplay)
                ? $"{entity.PriorityLevel} Â· {entity.Confidence}%"
                : entity.PriorityDisplay,
            PriorityLevel = entity.PriorityLevel,
            Confidence = entity.Confidence,
            Verification = entity.Verification,
            Assigned = entity.Assigned,
            Age = age,
            Status = entity.Status,
            Organisation = entity.Organisation,
            Description = entity.Description,
            AffectedPeopleCount = entity.AffectedPeopleCount,
            HasVulnerableMembers = entity.HasVulnerableMembers,
            IsLifeThreatening = entity.IsLifeThreatening,
            IsOverdue = entity.IsOverdue || entity.Status.Equals("Escalated", StringComparison.OrdinalIgnoreCase),
            IsPotentialDuplicate = entity.IsPotentialDuplicate,
            ContactName = entity.ContactName,
            ContactPhone = entity.ContactPhone,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static AdminDtos.AssistanceRequestDto MapToAdminDto(AssistanceRequest entity)
    {
        var now = DateTimeOffset.UtcNow;
        var diff = now - entity.CreatedAt;
        var timeAgo = diff.TotalMinutes < 60
            ? $"{(int)Math.Max(1, diff.TotalMinutes)} min ago"
            : diff.TotalHours < 24
                ? $"{(int)diff.TotalHours} hr ago"
                : $"{(int)diff.TotalDays} days ago";

        return new AdminDtos.AssistanceRequestDto
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt.UtcDateTime,
            TimeAgo = timeAgo,
            Category = entity.Category,
            Location = entity.Location,
            ReportedPriority = entity.PriorityLevel,
            Recommendation = $"{entity.PriorityLevel} priority",
            Confidence = $"{entity.Confidence}%",
            AssignedTo = entity.Assigned,
            Status = entity.Status,
            AffectedPeople = entity.AffectedPeopleCount,
            Contact = string.IsNullOrWhiteSpace(entity.ContactPhone)
                ? "Unverified"
                : $"Verified · ending {entity.ContactPhone[^Math.Min(4, entity.ContactPhone.Length)..]}",
            Description = entity.Description ?? string.Empty,
            CrisisName = entity.CrisisEvent
        };
    }

    #endregion
}
