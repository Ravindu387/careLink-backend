using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IActivityTrackingService
{
    Task<ActivityTrackingStatsDto> GetStatsAsync();
    Task<PaginatedResult<ActivityRecord>> GetActivitiesAsync(ActivityTrackingQueryDto query);
    Task<ActivityRecord?> GetByIdAsync(string id);
    Task<ActivityRecord> ManualCheckInAsync(ManualCheckInRequest request);
    Task<ActivityRecord> RecordActivityAsync(RecordActivityRequest request);
    Task<ActivityRecord?> UpdateActivityAsync(string id, UpdateActivityRequest request);
    Task<bool> DeleteActivityAsync(string id);
    Task<List<string>> GetLocationsAsync();
    Task<byte[]> ExportCsvAsync(ActivityTrackingQueryDto query);
}
