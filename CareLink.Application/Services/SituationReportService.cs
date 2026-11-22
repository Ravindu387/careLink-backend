using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class SituationReportService : ISituationReportService
{
    private readonly ISituationReportRepository _repository;
    private static int _reportNumberCounter = 25;
    private static readonly object _counterLock = new();

    public SituationReportService(ISituationReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<SituationReportDto>> GetAllAsync(
        SituationReportQueryParameters? query = null,
        CancellationToken cancellationToken = default)
    {
        var filter = query == null
            ? null
            : new SituationReportFilterParams
            {
                Search = query.Search,
                Event = query.Event,
                Status = query.Status,
                DateRange = query.DateRange
            };

        var reports = await _repository.GetAllAsync(filter, cancellationToken);
        return reports.Select(MapToDto).ToList();
    }

    public async Task<SituationReportDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var report = await _repository.GetByIdAsync(id, cancellationToken);
        return report == null ? null : MapToDto(report);
    }

    public async Task<SituationReportDto> CreateReportAsync(
        CreateSituationReportDto dto,
        CancellationToken cancellationToken = default)
    {
        string reportId;
        lock (_counterLock)
        {
            reportId = $"SITREP {_reportNumberCounter:D3}";
            _reportNumberCounter++;
        }

        var period = !string.IsNullOrWhiteSpace(dto.Period)
            ? dto.Period.Trim()
            : $"{DateTime.UtcNow:dd MMMM yyyy} · 08:00–12:00 UTC";

        var entity = new SituationReport
        {
            Id = reportId,
            Title = dto.Title.Trim(),
            Period = period,
            Status = "Draft",
            PreparedBy = !string.IsNullOrWhiteSpace(dto.PreparedBy) ? dto.PreparedBy.Trim() : "Operations Analysis Team",
            ApprovedBy = "Pending approval",
            Recipients = 0,
            Event = dto.Event.Trim(),
            ExecutiveSummary = dto.ExecutiveSummary?.Trim(),
            KeyHighlights = dto.KeyHighlights?.Trim(),
            OperationalChallenges = dto.OperationalChallenges?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<SituationReportDto?> UpdateStatusAsync(
        string id,
        UpdateReportStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var approvedBy = dto.ApprovedBy ?? (dto.Status == "Draft" ? "Pending approval" : "Amina Mensah");
        var recipients = dto.Recipients ?? (dto.Status == "Draft" ? 0 : 48);

        var updated = await _repository.UpdateStatusAsync(id, dto.Status, approvedBy, recipients, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<SituationReportMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
        return new SituationReportMetricsDto
        {
            PublishedCount = m.PublishedCount,
            DraftCount = m.DraftCount,
            TotalRecipients = m.TotalRecipients,
            NextReportDue = m.NextReportDue,
            NextReportId = m.NextReportId,
            LatestApproved = m.LatestApproved == null ? null : MapToDto(m.LatestApproved),
            TotalReports = m.TotalReports
        };
    }

    public async Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
        lock (_counterLock)
        {
            _reportNumberCounter = 25;
        }
    }

    private static SituationReportDto MapToDto(SituationReport r)
    {
        return new SituationReportDto
        {
            Id = r.Id,
            Title = r.Title,
            Period = r.Period,
            Status = r.Status,
            PreparedBy = r.PreparedBy,
            ApprovedBy = r.ApprovedBy,
            Recipients = r.Recipients,
            Event = r.Event,
            ExecutiveSummary = r.ExecutiveSummary,
            KeyHighlights = r.KeyHighlights,
            OperationalChallenges = r.OperationalChallenges,
            CreatedAt = r.CreatedAt,
            ApprovedAt = r.ApprovedAt,
            PublishedAt = r.PublishedAt
        };
    }
}
