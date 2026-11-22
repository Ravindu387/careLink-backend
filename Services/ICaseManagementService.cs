using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface ICaseManagementService
{
    Task<CaseDashboardResponse> GetDashboardDataAsync();
    Task<List<CaseItem>> GetCasesAsync(string? search = null, string? status = null, string? priority = null, string? assigned = null);
    Task<CaseItem?> GetCaseByIdAsync(string id);
    Task<CaseItem> CreateCaseAsync(CreateCaseRequest request);
    Task<CaseItem?> ClaimCaseAsync(string id, string? assignedTo = null);
    Task<CaseItem?> UpdateStatusAsync(string id, string status, string? notes = null);
    Task<byte[]> ExportSummaryCsvAsync();
}
