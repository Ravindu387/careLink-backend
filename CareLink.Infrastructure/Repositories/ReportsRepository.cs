using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class ReportsRepository : IReportsRepository
{
    private readonly CareLinkDbContext _context;

    public ReportsRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReportDefinition>> GetReportDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReportDefinitions
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Shelter>> GetSheltersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Shelters
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccommodationRequest>> GetAccommodationRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationRequests
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Admission>> GetAdmissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Admissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
