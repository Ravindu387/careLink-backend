using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IUnassignedCasesService
{
    Task<UnassignedCasesStatsDto> GetStatsAsync();
    Task<PaginatedResult<UnassignedCase>> GetCasesAsync(UnassignedCasesQueryDto query);
    Task<UnassignedCase?> GetByIdAsync(string id);
    Task<int> ClaimCasesAsync(ClaimUnassignedCasesRequest request);
    Task<UnassignedCase?> AssignCaseAsync(AssignUnassignedCaseRequest request);
    Task<UnassignedCase?> UpdateCaseAsync(string id, UpdateUnassignedCaseRequest request);
    Task<byte[]> ExportCsvAsync(UnassignedCasesQueryDto query);
}
