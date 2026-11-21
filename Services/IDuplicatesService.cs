using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IDuplicatesService
{
    Task<DuplicatesStatsDto> GetStatsAsync();
    Task<PaginatedResult<DuplicateCaseItem>> GetDuplicatesAsync(DuplicateQueryDto query);
    Task<DuplicateCaseItem?> GetByIdAsync(string id);
    Task<DuplicateCaseItem?> UpdateStatusAsync(string id, UpdateDuplicateStatusRequest request);
    Task<bool> MergeCasesAsync(MergeCasesRequest request);
    Task<byte[]> ExportCsvAsync(DuplicateQueryDto query);
}
