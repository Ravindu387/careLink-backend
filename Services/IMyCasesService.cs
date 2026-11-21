using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IMyCasesService
{
    Task<MyCasesStatsDto> GetStatsAsync();
    Task<PaginatedResult<MyCaseItem>> GetCasesAsync(MyCasesQueryDto query);
    Task<MyCaseItem?> GetCaseByIdAsync(string id);
    Task<MyCaseItem> CreateCaseAsync(CreateMyCaseRequest request);
    Task<MyCaseItem?> UpdateStatusAsync(string id, UpdateMyCaseStatusRequest request);
    Task<int> BulkActionAsync(BulkMyCasesActionRequest request);
    Task<byte[]> ExportCsvAsync(MyCasesQueryDto query);
}
