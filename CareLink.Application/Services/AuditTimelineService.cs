using System.Security.Cryptography;
using System.Text;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class AuditTimelineService : IAuditTimelineService
{
    private readonly IAuditTimelineRepository _repository;
    private static int _auditCounter = 482200;
    private static readonly object _lock = new();

    public AuditTimelineService(IAuditTimelineRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AuditEventResponseDto>> GetAllAsync(
        AuditQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var filter = new AuditFilterParams
        {
            Search = query.Search,
            Action = query.Action ?? query.ActionType,
            ActionType = query.ActionType,
            Actor = query.Actor ?? query.User,
            Type = query.Type ?? query.RecordType,
            RecordType = query.RecordType,
            DateRange = query.DateRange,
            DateFrom = query.DateFrom,
            DateTo = query.DateTo
        };

        var list = await _repository.GetAllAsync(filter, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<AuditEventResponseDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<AuditEventResponseDto> LogEventAsync(
        CreateAuditEventDto dto,
        CancellationToken cancellationToken = default)
    {
        string id;
        lock (_lock)
        {
            _auditCounter++;
            id = $"AUD-{_auditCounter}";
        }

        var now = DateTimeOffset.UtcNow;
        var checksum = ComputeSha256Hash($"{id}|{dto.Action}|{dto.Actor}|{dto.Record}|{dto.Current}|{dto.Rationale}|{now:O}");

        var entity = new AuditEvent
        {
            Id = id,
            Action = dto.Action.Trim(),
            Actor = dto.Actor.Trim(),
            Record = dto.Record.Trim(),
            Previous = string.IsNullOrWhiteSpace(dto.Previous) ? "-" : dto.Previous.Trim(),
            Current = dto.Current.Trim(),
            Rationale = dto.Rationale.Trim(),
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "AI Decision" : dto.Type.Trim(),
            IpAddress = string.IsNullOrWhiteSpace(dto.IpAddress) ? "127.0.0.1" : dto.IpAddress.Trim(),
            Checksum = checksum,
            IsTamperProofVerified = true,
            CreatedAt = now
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<AuditTimelineMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _repository.GetMetricsAsync(cancellationToken);
        return new AuditTimelineMetricsDto
        {
            TotalEvents = metrics.TotalEvents,
            EventsToday = metrics.EventsToday,
            AIDecisionEvents = metrics.AIDecisionEvents,
            HumanOverrideEvents = metrics.HumanOverrideEvents,
            IntegrityScore = metrics.IntegrityScore,
            RetentionPeriod = metrics.RetentionPeriod,
            AllRecordsVerified = metrics.AllRecordsVerified
        };
    }

    public async Task<bool> VerifyLogIntegrityAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.VerifyLogIntegrityAsync(cancellationToken);
    }

    public async Task ResetDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static AuditEventResponseDto MapToDto(AuditEvent entity)
    {
        var local = entity.CreatedAt.ToLocalTime();
        var timestamp = $"{local:dd MMM} · {local:HH:mm}";
        var date = entity.CreatedAt.ToString("yyyy-MM-dd");

        var change = !string.IsNullOrWhiteSpace(entity.Previous) && entity.Previous != "-"
            ? $"{entity.Previous} → {entity.Current}"
            : (!string.IsNullOrWhiteSpace(entity.Current) ? entity.Current : "-");

        return new AuditEventResponseDto
        {
            Id = entity.Id,
            Action = entity.Action,
            Actor = entity.Actor,
            Timestamp = timestamp,
            Record = entity.Record,
            Previous = entity.Previous,
            Current = entity.Current,
            Change = change,
            Rationale = entity.Rationale,
            Type = entity.Type,
            ActionType = entity.Type,
            RecordType = entity.Type,
            Date = date,
            IpAddress = entity.IpAddress,
            Checksum = entity.Checksum,
            IsVerified = entity.IsTamperProofVerified,
            CreatedAt = entity.CreatedAt
        };
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
