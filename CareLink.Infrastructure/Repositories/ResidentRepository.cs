using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class ResidentRepository : IResidentRepository
{
    private readonly CareLinkDbContext _context;

    public ResidentRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<Resident?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Residents
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Resident>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Residents
            .AsNoTracking()
            .OrderByDescending(r => r.CheckIn)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Resident> Items, int TotalCount)> GetPagedAsync(
        ResidentQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Residents.AsNoTracking().AsQueryable();

        // Search text
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(search) ||
                r.Id.ToLower().Contains(search) ||
                r.FamilyGroup.ToLower().Contains(search) ||
                r.Contact.ToLower().Contains(search));
        }

        // Shelter filter
        if (!string.IsNullOrWhiteSpace(parameters.ShelterId) &&
            !parameters.ShelterId.Equals("All shelters", StringComparison.OrdinalIgnoreCase))
        {
            var shelter = parameters.ShelterId.Trim().ToLower();
            query = query.Where(r =>
                r.ShelterId.ToLower() == shelter ||
                r.ShelterName.ToLower() == shelter);
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim().ToLower();
            query = query.Where(r => r.Status.ToLower() == status);
        }

        // Assistance filter
        if (!string.IsNullOrWhiteSpace(parameters.AssistanceFilter) &&
            !parameters.AssistanceFilter.Equals("All assistance needs", StringComparison.OrdinalIgnoreCase))
        {
            var filter = parameters.AssistanceFilter.Trim().ToLower();
            if (filter.Contains("needs assistance") || filter == "assistance")
            {
                query = query.Where(r => r.AssistanceNeeds != "None" || r.Status.ToLower() == "assistance");
            }
            else if (filter.Contains("no assistance"))
            {
                query = query.Where(r => r.AssistanceNeeds == "None" && r.Status.ToLower() != "assistance");
            }
        }

        // Gender filter
        if (!string.IsNullOrWhiteSpace(parameters.Gender) &&
            !parameters.Gender.Equals("All genders", StringComparison.OrdinalIgnoreCase))
        {
            var gender = parameters.Gender.Trim().ToLower();
            query = query.Where(r => r.Gender.ToLower() == gender);
        }

        // Age filters
        if (parameters.MinAge.HasValue)
        {
            query = query.Where(r => r.Age >= parameters.MinAge.Value);
        }

        if (parameters.MaxAge.HasValue)
        {
            query = query.Where(r => r.Age <= parameters.MaxAge.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = await query
            .OrderByDescending(r => r.CheckIn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(int TotalRegistered, int TotalShelters, int FamilyGroupsCount, int SingleResidentsCount, int AssistanceNeedsCount, int CheckedOutTodayCount, int CheckedInTodayCount)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var totalRegistered = await _context.Residents.CountAsync(cancellationToken);

        var totalShelters = await _context.Residents
            .Where(r => !string.IsNullOrEmpty(r.ShelterId))
            .Select(r => r.ShelterId)
            .Distinct()
            .CountAsync(cancellationToken);

        var familyGroupsCount = await _context.Residents
            .Where(r => !string.IsNullOrEmpty(r.FamilyGroup) && r.FamilyGroup != "Individual")
            .Select(r => r.FamilyGroup)
            .Distinct()
            .CountAsync(cancellationToken);

        var singleResidentsCount = await _context.Residents
            .CountAsync(r => string.IsNullOrEmpty(r.FamilyGroup) || r.FamilyGroup == "Individual", cancellationToken);

        var assistanceNeedsCount = await _context.Residents
            .CountAsync(r => r.AssistanceNeeds != "None" || r.Status == "Assistance", cancellationToken);

        var today = DateTime.UtcNow.Date;

        var checkedOutTodayCount = await _context.Residents
            .CountAsync(r => r.Status == "Checked Out" &&
                ((r.CheckedOutAt.HasValue && r.CheckedOutAt.Value >= today) || r.CheckIn >= today), cancellationToken);

        var checkedInTodayCount = await _context.Residents
            .CountAsync(r => r.CheckIn >= today, cancellationToken);

        return (
            totalRegistered,
            totalShelters,
            familyGroupsCount,
            singleResidentsCount,
            assistanceNeedsCount,
            checkedOutTodayCount,
            checkedInTodayCount
        );
    }

    public async Task<Resident> AddAsync(Resident resident, CancellationToken cancellationToken = default)
    {
        await _context.Residents.AddAsync(resident, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return resident;
    }

    public async Task<Resident?> UpdateAsync(Resident resident, CancellationToken cancellationToken = default)
    {
        _context.Residents.Update(resident);
        await _context.SaveChangesAsync(cancellationToken);
        return resident;
    }

    public async Task<bool> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Residents.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (existing == null) return false;

        existing.Status = status;
        if (status.Equals("Checked Out", StringComparison.OrdinalIgnoreCase))
        {
            existing.CheckedOutAt = DateTime.UtcNow;
        }
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CheckoutResidentAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Residents.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (existing == null) return false;

        existing.Status = "Checked Out";
        existing.CheckedOutAt = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Residents.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.Residents.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
