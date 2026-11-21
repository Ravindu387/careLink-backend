using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class AssistanceRequestService : IAssistanceRequestService
{
    private readonly IAssistanceRequestRepository _repository;
    private readonly IAIInferenceEngine _aiEngine;
    private static int _requestCounter = 2855;
    private static readonly object _counterLock = new();

    public AssistanceRequestService(
        IAssistanceRequestRepository repository,
        IAIInferenceEngine aiEngine)
    {
        _repository = repository;
        _aiEngine = aiEngine;
    }

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
        CreateAssistanceRequestDto dto,
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
        UpdateAssistanceRequestDto dto,
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
                ? $"{entity.PriorityLevel} · {entity.Confidence}%"
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
}
