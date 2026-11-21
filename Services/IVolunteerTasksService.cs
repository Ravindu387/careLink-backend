using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IVolunteerTasksService
{
    Task<VolunteerTasksOverviewDto> GetTasksOverviewAsync(VolunteerTaskQueryDto query);
    Task<VolunteerTaskTabsCountDto> GetTabCountsAsync();
    Task<VolunteerTaskDto?> GetTaskByIdAsync(string id);
    Task<VolunteerTaskDto> CreateTaskAsync(CreateVolunteerTaskRequest request);
    Task<VolunteerTaskDto?> UpdateTaskAsync(string id, UpdateVolunteerTaskRequest request);
    Task<VolunteerTaskDto?> UpdateTaskStatusAsync(string id, string status);
    Task<VolunteerTaskDto?> AdjustAssignedVolunteersAsync(string id, int delta);
    Task<bool> DeleteTaskAsync(string id);
    Task<byte[]> ExportTasksCsvAsync(VolunteerTaskQueryDto query);
}
