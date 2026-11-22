using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface ICampaignService
{
    Task<IEnumerable<CampaignDto>> GetAllCampaignsAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<CampaignDto>> GetPagedCampaignsAsync(CampaignQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<CampaignDto?> GetCampaignByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<CampaignDto> CreateCampaignAsync(CreateCampaignDto dto, CancellationToken cancellationToken = default);
    Task<CampaignDto?> UpdateCampaignAsync(string id, UpdateCampaignDto dto, CancellationToken cancellationToken = default);
    Task<CampaignDto?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);
    Task<bool> DeleteCampaignAsync(string id, CancellationToken cancellationToken = default);
    Task<CampaignSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
