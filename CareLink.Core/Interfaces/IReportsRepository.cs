using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IReportsRepository
{
    Task<IEnumerable<ReportDefinition>> GetReportDefinitionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Shelter>> GetSheltersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AccommodationRequest>> GetAccommodationRequestsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Admission>> GetAdmissionsAsync(CancellationToken cancellationToken = default);
}
