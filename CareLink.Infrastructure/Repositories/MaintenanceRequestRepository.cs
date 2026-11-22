using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class MaintenanceRequestRepository : IMaintenanceRequestRepository
{
    private readonly CareLinkDbContext _context;

    public MaintenanceRequestRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<MaintenanceRequest> Items, int TotalCount)> GetPagedAsync(
        MaintenanceRequestQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MaintenanceRequests.AsNoTracking().AsQueryable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim().ToLower();
            query = query.Where(r =>
                r.Id.ToLower().Contains(term) ||
                r.Issue.ToLower().Contains(term) ||
                r.Shelter.ToLower().Contains(term) ||
                (r.Room != null && r.Room.ToLower().Contains(term)) ||
                (r.Technician != null && r.Technician.ToLower().Contains(term)) ||
                (r.Description != null && r.Description.ToLower().Contains(term)));
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(parameters.Category) &&
            !parameters.Category.Equals("All", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
        {
            var category = parameters.Category.Trim();
            query = query.Where(r => r.Category.ToLower() == category.ToLower());
        }

        // Priority filter
        if (!string.IsNullOrWhiteSpace(parameters.Priority) &&
            !parameters.Priority.Equals("All", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
        {
            var priority = parameters.Priority.Trim();
            query = query.Where(r => r.Priority.ToLower() == priority.ToLower());
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim();
            query = query.Where(r => r.Status.ToLower() == status.ToLower());
        }

        // Technician filter
        if (!string.IsNullOrWhiteSpace(parameters.Technician) &&
            !parameters.Technician.Equals("All", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Technician.Equals("All technicians", StringComparison.OrdinalIgnoreCase))
        {
            var technician = parameters.Technician.Trim();
            query = query.Where(r => r.Technician.ToLower() == technician.ToLower());
        }

        // Shelter filter
        if (!string.IsNullOrWhiteSpace(parameters.Shelter) &&
            !parameters.Shelter.Equals("All", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Shelter.Equals("All shelters", StringComparison.OrdinalIgnoreCase))
        {
            var shelter = parameters.Shelter.Trim();
            query = query.Where(r => r.Shelter.ToLower() == shelter.ToLower());
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = await query
            .OrderByDescending(r => r.ReportedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<MaintenanceRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceRequests
            .AsNoTracking()
            .OrderByDescending(r => r.ReportedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<MaintenanceRequest> AddAsync(MaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        await _context.MaintenanceRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<bool> UpdateAsync(MaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.MaintenanceRequests.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (existing == null) return false;

        if (!ReferenceEquals(existing, request))
        {
            _context.Entry(existing).CurrentValues.SetValues(request);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.MaintenanceRequests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.MaintenanceRequests.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceRequests
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }
}
