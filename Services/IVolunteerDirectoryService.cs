using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IVolunteerDirectoryService
{
    Task<VolunteerDirectoryOverviewDto> GetOverviewAsync(VolunteerDirectoryQueryDto query);
    Task<VolunteerDirectoryStatsDto> GetStatsAsync();
    Task<VolunteerDirectoryItemDto?> GetByIdAsync(string id);
    Task<VolunteerDirectoryItemDto> CreateAsync(CreateVolunteerDirectoryRequest request);
    Task<VolunteerDirectoryItemDto?> UpdateAsync(string id, UpdateVolunteerDirectoryRequest request);
    Task<VolunteerDirectoryItemDto?> UpdateVerificationAsync(string id, string verification);
    Task<VolunteerDirectoryItemDto?> UpdateAvailabilityAsync(string id, string availability);
    Task<bool> DeleteAsync(string id);
    Task<byte[]> ExportCsvAsync(VolunteerDirectoryQueryDto query);
}
