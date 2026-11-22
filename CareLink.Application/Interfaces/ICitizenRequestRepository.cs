namespace CareLink.Application.Interfaces;

using CareLink.Application.DTOs;
using CareLink.Core.Entities;

public interface ICitizenRequestRepository
{
    Task<IEnumerable<CitizenRequest>> GetRequestsAsync(string? citizenName = null, string? search = null, string? status = null, string? category = null);
    Task<CitizenRequest?> GetByIdAsync(string id);
    Task<CitizenRequest> CreateRequestAsync(CitizenRequest request);
    Task<CitizenRequest?> UpdateStatusAsync(string id, string status, int? progress = null);
    Task<bool> DeleteRequestAsync(string id);
    Task<CitizenRequestsSummaryDto> GetSummaryAsync(string? citizenName = null);
}
