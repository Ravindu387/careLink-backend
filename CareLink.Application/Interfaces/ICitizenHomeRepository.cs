namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface ICitizenHomeRepository
{
    Task<CrisisOperation> GetActiveOperationAsync();
    Task<IEnumerable<CitizenRequest>> GetCitizenRequestsAsync(string? citizenName = null, string? search = null, string? status = null);
    Task<CitizenRequest?> GetCitizenRequestByIdAsync(string id);
    Task<CitizenRequest> CreateCitizenRequestAsync(CitizenRequest request);
    Task<CitizenRequest?> UpdateCitizenRequestStatusAsync(string id, string status);
    Task<bool> DeleteCitizenRequestAsync(string id);
    Task<IEnumerable<NearbyFacility>> GetNearbyFacilitiesAsync(string? search = null);
    Task<NearbyFacility?> GetFacilityByIdAsync(string id);
    Task<NearbyFacility> CreateFacilityAsync(NearbyFacility facility);
    Task<NearbyFacility?> UpdateFacilityStatusAsync(string id, string status);
}
