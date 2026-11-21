using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaignRepository;

    public CampaignService(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<IEnumerable<CampaignDto>> GetAllCampaignsAsync(CancellationToken cancellationToken = default)
    {
        var campaigns = await _campaignRepository.GetAllAsync(cancellationToken);
        return campaigns.Select(MapToDto);
    }

    public async Task<PagedResultDto<CampaignDto>> GetPagedCampaignsAsync(CampaignQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _campaignRepository.GetPagedAsync(parameters, cancellationToken);

        var page = parameters.Page.GetValueOrDefault(1);
        if (page < 1) page = 1;
        var pageSize = parameters.PageSize.GetValueOrDefault(10);
        if (pageSize < 1) pageSize = 10;

        return new PagedResultDto<CampaignDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CampaignDto?> GetCampaignByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (campaign == null) return null;
        return MapToDto(campaign);
    }

    public async Task<CampaignDto> CreateCampaignAsync(CreateCampaignDto dto, CancellationToken cancellationToken = default)
    {
        var all = await _campaignRepository.GetAllAsync(cancellationToken);
        var maxIdNum = all
            .Select(c => c.Id.Replace("CAM-", ""))
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(100)
            .Max();

        var id = $"CAM-{(maxIdNum + 1)}";

        DateTime.TryParse(dto.StartDate, out var parsedStart);
        if (parsedStart == default) parsedStart = DateTime.UtcNow.Date;

        DateTime.TryParse(dto.EndDate, out var parsedEnd);
        if (parsedEnd == default) parsedEnd = parsedStart.AddDays(30);

        var period = $"{parsedStart.ToString("dd MMM", CultureInfo.InvariantCulture)}–{parsedEnd.ToString("dd MMM", CultureInfo.InvariantCulture)}";

        var campaign = new Campaign
        {
            Id = id,
            Name = dto.Name.Trim(),
            Event = string.IsNullOrWhiteSpace(dto.Event) ? "Monsoon Floods 2025" : dto.Event.Trim(),
            Period = period,
            Target = dto.Target,
            Raised = 0,
            Status = "Active",
            StartDate = parsedStart,
            EndDate = parsedEnd,
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _campaignRepository.AddAsync(campaign, cancellationToken);
        return MapToDto(created);
    }

    public async Task<CampaignDto?> UpdateCampaignAsync(string id, UpdateCampaignDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) existing.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Event)) existing.Event = dto.Event.Trim();
        if (dto.Target.HasValue && dto.Target.Value > 0) existing.Target = dto.Target.Value;
        if (dto.Raised.HasValue && dto.Raised.Value >= 0) existing.Raised = dto.Raised.Value;
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Description)) existing.Description = dto.Description.Trim();

        var datesChanged = false;
        if (!string.IsNullOrWhiteSpace(dto.StartDate) && DateTime.TryParse(dto.StartDate, out var newStart))
        {
            existing.StartDate = newStart;
            datesChanged = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.EndDate) && DateTime.TryParse(dto.EndDate, out var newEnd))
        {
            existing.EndDate = newEnd;
            datesChanged = true;
        }

        if (datesChanged)
        {
            existing.Period = $"{existing.StartDate.ToString("dd MMM", CultureInfo.InvariantCulture)}–{existing.EndDate.ToString("dd MMM", CultureInfo.InvariantCulture)}";
        }

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _campaignRepository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<CampaignDto?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        var existing = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Status = status.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _campaignRepository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteCampaignAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _campaignRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<CampaignSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var (activeCampaigns, combinedTarget, totalRaised, endingSoon) = await _campaignRepository.GetSummaryMetricsAsync(cancellationToken);

        return new CampaignSummaryDto
        {
            ActiveCampaigns = activeCampaigns,
            CombinedTarget = combinedTarget,
            TotalRaised = totalRaised,
            EndingSoon = endingSoon
        };
    }

    private static CampaignDto MapToDto(Campaign c)
    {
        return new CampaignDto
        {
            Id = c.Id,
            Name = c.Name,
            Event = c.Event,
            Period = c.Period,
            Raised = c.Raised,
            Target = c.Target,
            Status = c.Status,
            StartDate = c.StartDate.ToString("yyyy-MM-dd"),
            EndDate = c.EndDate.ToString("yyyy-MM-dd"),
            Description = c.Description
        };
    }
}
