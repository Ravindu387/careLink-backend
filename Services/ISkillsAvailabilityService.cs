using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface ISkillsAvailabilityService
{
    Task<SkillsAvailabilityOverviewDto> GetOverviewAsync(string? area = null);
    Task<SkillsAvailabilityStatsDto> GetStatsAsync();
    Task<List<SkillCoverageItemDto>> GetSkillsAsync(string? search = null);
    Task<SkillCoverageItemDto?> GetSkillByNameAsync(string name);
    Task<SkillCoverageItemDto> AddSkillAsync(CreateSkillRequest request);
    Task<bool> RemoveSkillAsync(string name);
    Task<List<AvailabilityForecastDto>> GetForecastAsync(string? area = null);
    Task<AvailabilityRequestDto> SubmitAvailabilityRequestAsync(AvailabilityRequestDto request);
    Task<List<AvailabilityRequestDto>> GetAvailabilityRequestsAsync();
    Task<byte[]> ExportSkillsCsvAsync();
}
