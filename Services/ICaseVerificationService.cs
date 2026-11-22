using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface ICaseVerificationService
{
    Task<VerificationAnalyticsResponse> GetAnalyticsAsync();
    Task<PaginatedResult<VerificationCaseItem>> GetCasesAsync(VerificationQueryDto query);
    Task<VerificationCaseItem?> GetCaseByIdAsync(string id);
    Task<VerificationCaseItem?> RecordDecisionAsync(string id, VerificationDecisionRequest request);
    Task<byte[]> ExportVerificationCsvAsync(VerificationQueryDto query);
}
