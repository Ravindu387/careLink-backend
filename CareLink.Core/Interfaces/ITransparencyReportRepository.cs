using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface ITransparencyReportRepository
{
    Task<(IEnumerable<TransparencyReport> Items, int TotalCount)> GetPagedAsync(
        TransparencyReportQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TransparencyReport>> GetAllAsync(
        TransparencyReportQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<TransparencyReport?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<TransparencyReport> AddAsync(TransparencyReport report, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(TransparencyReport report, CancellationToken cancellationToken = default);

    Task<bool> PublishAsync(string id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
