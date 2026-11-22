using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class AccommodationRequestRepository : IAccommodationRequestRepository
{
    private readonly CareLinkDbContext _context;

    public AccommodationRequestRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<AccommodationRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AccommodationRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationRequests
            .AsNoTracking()
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<AccommodationRequest> Items, int TotalCount)> GetPagedAsync(
        AccommodationRequestQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AccommodationRequests.AsNoTracking().AsQueryable();

        // Search text
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(r =>
                r.Id.ToLower().Contains(search) ||
                r.Applicant.ToLower().Contains(search) ||
                r.CurrentLocation.ToLower().Contains(search) ||
                r.RequestedShelter.ToLower().Contains(search));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Status.Equals("All requests", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim();
            query = query.Where(r => r.Status.ToLower() == status.ToLower());
        }

        // Priority filter
        if (!string.IsNullOrWhiteSpace(parameters.Priority) &&
            !parameters.Priority.Equals("All priorities", StringComparison.OrdinalIgnoreCase))
        {
            var priority = parameters.Priority.Trim();
            query = query.Where(r => r.Priority.ToLower() == priority.ToLower());
        }

        // Shelter filter
        if (!string.IsNullOrWhiteSpace(parameters.ShelterId) &&
            !parameters.ShelterId.Equals("All shelters", StringComparison.OrdinalIgnoreCase))
        {
            var shelterId = parameters.ShelterId.Trim();
            query = query.Where(r => r.ShelterId.ToLower() == shelterId.ToLower() ||
                                     r.RequestedShelter.ToLower() == shelterId.ToLower());
        }

        // Min family members
        if (parameters.MinFamilyMembers.HasValue)
        {
            query = query.Where(r => r.FamilyMembers >= parameters.MinFamilyMembers.Value);
        }

        // Max family members
        if (parameters.MaxFamilyMembers.HasValue)
        {
            query = query.Where(r => r.FamilyMembers <= parameters.MaxFamilyMembers.Value);
        }

        // Location text
        if (!string.IsNullOrWhiteSpace(parameters.Location))
        {
            var loc = parameters.Location.Trim().ToLower();
            query = query.Where(r => r.CurrentLocation.ToLower().Contains(loc));
        }

        // Date range
        if (!string.IsNullOrWhiteSpace(parameters.DateRange))
        {
            var range = parameters.DateRange.Trim().ToLower();
            if (range == "last 7 days" || range == "last7days")
            {
                var cutoff = DateTime.UtcNow.AddDays(-7);
                query = query.Where(r => r.RequestDate >= cutoff);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 8;

        var items = await query
            .OrderByDescending(r => r.RequestDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationRequests
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<AccommodationRequest> AddAsync(AccommodationRequest request, CancellationToken cancellationToken = default)
    {
        await _context.AccommodationRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<bool> UpdateStatusAsync(string id, string newStatus, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AccommodationRequests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (existing == null) return false;

        existing.Status = newStatus;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AccommodationRequests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.AccommodationRequests.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
