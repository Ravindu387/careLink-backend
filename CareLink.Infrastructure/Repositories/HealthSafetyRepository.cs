using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class HealthSafetyRepository : IHealthSafetyRepository
{
    private readonly CareLinkDbContext _context;

    public HealthSafetyRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HealthSafetyIncident>> GetAllAsync(
        HealthSafetyIncidentQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.HealthSafetyIncidents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Id.ToLower().Contains(term) ||
                x.Type.ToLower().Contains(term) ||
                x.Location.ToLower().Contains(term) ||
                x.ReportedBy.ToLower().Contains(term) ||
                (x.AssignedStaff != null && x.AssignedStaff.ToLower().Contains(term)) ||
                x.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Type) &&
            !parameters.Type.StartsWith("All", StringComparison.OrdinalIgnoreCase))
        {
            var type = parameters.Type.Trim();
            query = query.Where(x => x.Type.ToLower() == type.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(parameters.Severity) &&
            !parameters.Severity.StartsWith("All", StringComparison.OrdinalIgnoreCase))
        {
            var severity = parameters.Severity.Trim();
            query = query.Where(x => x.Severity.ToLower() == severity.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.StartsWith("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim();
            query = query.Where(x => x.Status.ToLower() == status.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(parameters.DateRange) &&
            !parameters.DateRange.StartsWith("Date", StringComparison.OrdinalIgnoreCase) &&
            !parameters.DateRange.StartsWith("All", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.UtcNow;
            if (parameters.DateRange.Equals("Today", StringComparison.OrdinalIgnoreCase))
            {
                var today = now.Date;
                query = query.Where(x => x.IncidentDate >= today);
            }
            else if (parameters.DateRange.Equals("Last 7 days", StringComparison.OrdinalIgnoreCase))
            {
                var sevenDaysAgo = now.AddDays(-7);
                query = query.Where(x => x.IncidentDate >= sevenDaysAgo);
            }
            else if (parameters.DateRange.Equals("Last 30 days", StringComparison.OrdinalIgnoreCase))
            {
                var thirtyDaysAgo = now.AddDays(-30);
                query = query.Where(x => x.IncidentDate >= thirtyDaysAgo);
            }
        }

        return await query
            .OrderByDescending(x => x.IncidentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<HealthSafetyIncident?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.HealthSafetyIncidents
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<HealthSafetyIncident> AddAsync(HealthSafetyIncident incident, CancellationToken cancellationToken = default)
    {
        await _context.HealthSafetyIncidents.AddAsync(incident, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return incident;
    }

    public async Task<bool> UpdateAsync(HealthSafetyIncident incident, CancellationToken cancellationToken = default)
    {
        var existing = await _context.HealthSafetyIncidents.FirstOrDefaultAsync(x => x.Id == incident.Id, cancellationToken);
        if (existing == null) return false;

        if (!ReferenceEquals(existing, incident))
        {
            _context.Entry(existing).CurrentValues.SetValues(incident);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.HealthSafetyIncidents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.HealthSafetyIncidents.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.HealthSafetyIncidents
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<IEnumerable<SafetyCheck>> GetSafetyChecksAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SafetyChecks
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
