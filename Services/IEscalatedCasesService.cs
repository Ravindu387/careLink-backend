using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IEscalatedCasesService
{
    Task<EscalationStatsDto> GetStatsAsync();
    Task<PaginatedResult<EscalatedCaseItem>> GetCasesAsync(EscalatedQueryDto query);
    Task<EscalatedCaseItem?> GetCaseByIdAsync(string id);
    Task<int> BulkActionAsync(BulkEscalationActionRequest request);
    Task<EscalatedCaseItem?> RecordDecisionAsync(string id, EscalationDecisionRequest request);
    Task<byte[]> ExportCsvAsync(EscalatedQueryDto query);
}
