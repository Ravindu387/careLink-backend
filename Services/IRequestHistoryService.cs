using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IRequestHistoryService
{
    Task<RequestHistoryStatsDto> GetStatsAsync();
    Task<PaginatedResult<HistoryEventItem>> GetEventsAsync(RequestHistoryQueryDto query);
    Task<HistoryEventItem?> GetByIdAsync(string id);
    Task<HistoryEventItem> LogEventAsync(LogHistoryEventRequest request);
    Task<List<string>> GetUniqueActorsAsync();
    Task<List<string>> GetUniqueCaseIdsAsync();
    Task<byte[]> ExportCsvAsync(RequestHistoryQueryDto query);
}
