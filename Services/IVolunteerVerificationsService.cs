using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IVolunteerVerificationsService
{
    Task<VolunteerVerificationStatsDto> GetStatsAsync();
    Task<List<VolunteerVerificationApplication>> GetPendingApplicationsAsync();
    Task<PaginatedResult<VolunteerVerificationApplication>> GetApplicationsAsync(VolunteerVerificationQueryDto query);
    Task<VolunteerVerificationApplication?> GetByIdAsync(string id);
    Task<VolunteerVerificationApplication?> ApproveAsync(string id, ReviewVerificationRequest? request = null);
    Task<VolunteerVerificationApplication?> RejectAsync(string id, ReviewVerificationRequest? request = null);
    Task<VolunteerVerificationApplication> CreateAsync(CreateVerificationApplicationRequest request);
    Task<bool> DeleteAsync(string id);
    Task<byte[]> ExportCsvAsync(VolunteerVerificationQueryDto query);
}
