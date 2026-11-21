using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface ITrainingCertificationsService
{
    Task<TrainingOverviewDto> GetOverviewAsync();
    Task<List<TrainingDto>> GetTrainingsAsync(string? search = null, string? status = null, string? requirement = null);
    Task<TrainingDto?> GetTrainingByIdAsync(string id);
    Task<TrainingDto> CreateTrainingAsync(CreateTrainingRequest request);
    Task<TrainingDto?> UpdateTrainingStatusAsync(string id, string status);
    Task<bool> DeleteTrainingAsync(string id);
    Task<byte[]> ExportTrainingsCsvAsync(string? search = null, string? status = null);

    Task<List<CertificateDto>> GetCertificatesAsync(string? search = null, string? status = null);
    Task<CertificateDto?> GetCertificateByIdAsync(string id);
    Task<CertificateDto> CreateCertificateAsync(CreateCertificateRequest request);
    Task<CertificateDto?> UpdateCertificateStatusAsync(string id, string status);
    Task<bool> DeleteCertificateAsync(string id);
    Task<byte[]> ExportCertificatesCsvAsync(string? search = null, string? status = null);
}
