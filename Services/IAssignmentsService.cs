using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IAssignmentsService
{
    Task<AssignmentsStatsDto> GetStatsAsync();
    Task<PaginatedResult<AssignmentItem>> GetAssignmentsAsync(AssignmentsQueryDto query);
    Task<AssignmentItem?> GetByIdAsync(string id);
    Task<AssignmentItem> CreateAssignmentAsync(CreateAssignmentRequest request);
    Task<AssignmentItem?> UpdateAssignmentAsync(string id, UpdateAssignmentRequest request);
    Task<AssignmentOptionsDto> GetOptionsAsync();
    Task<byte[]> ExportCsvAsync(AssignmentsQueryDto query);
}
