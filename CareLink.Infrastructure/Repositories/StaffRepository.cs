using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly CareLinkDbContext _context;

    public StaffRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StaffMember>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StaffMembers
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<StaffMember> Items, int TotalCount)> GetPagedAsync(
        StaffQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.StaffMembers.AsNoTracking().AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                s.Id.ToLower().Contains(search) ||
                s.Designation.ToLower().Contains(search) ||
                s.Email.ToLower().Contains(search) ||
                s.Contact.ToLower().Contains(search));
        }

        // Shelter
        if (!string.IsNullOrWhiteSpace(parameters.Shelter) &&
            !parameters.Shelter.Equals("All shelters", StringComparison.OrdinalIgnoreCase))
        {
            var shelter = parameters.Shelter.Trim().ToLower();
            query = query.Where(s => s.Shelter.ToLower() == shelter);
        }

        // Designation
        if (!string.IsNullOrWhiteSpace(parameters.Designation) &&
            !parameters.Designation.Equals("All designations", StringComparison.OrdinalIgnoreCase))
        {
            var designation = parameters.Designation.Trim().ToLower();
            query = query.Where(s => s.Designation.ToLower() == designation);
        }

        // Shift
        if (!string.IsNullOrWhiteSpace(parameters.Shift) &&
            !parameters.Shift.Equals("All shifts", StringComparison.OrdinalIgnoreCase))
        {
            var shift = parameters.Shift.Trim().ToLower();
            query = query.Where(s => s.Shift.ToLower() == shift);
        }

        // Availability
        if (!string.IsNullOrWhiteSpace(parameters.Availability) &&
            !parameters.Availability.Equals("All availability", StringComparison.OrdinalIgnoreCase))
        {
            var availability = parameters.Availability.Trim().ToLower();
            query = query.Where(s => s.Availability.ToLower() == availability);
        }

        // Status
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim().ToLower();
            query = query.Where(s => s.Status.ToLower() == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = await query
            .OrderBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<StaffMember?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.StaffMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<StaffMember> AddAsync(StaffMember staff, CancellationToken cancellationToken = default)
    {
        await _context.StaffMembers.AddAsync(staff, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return staff;
    }

    public async Task<StaffMember?> UpdateAsync(StaffMember staff, CancellationToken cancellationToken = default)
    {
        _context.StaffMembers.Update(staff);
        await _context.SaveChangesAsync(cancellationToken);
        return staff;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.StaffMembers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.StaffMembers.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(int TotalStaff, int OnDuty, int NextShift, int Unavailable)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var staff = await _context.StaffMembers.AsNoTracking().ToListAsync(cancellationToken);

        var totalStaff = staff.Count(s => s.Status == "Active");
        var onDuty = staff.Count(s => s.Status == "Active" && s.Availability == "On duty");
        var unavailable = staff.Count(s => s.Status == "Active" && (s.Availability == "Leave" || s.Availability == "Absent"));
        var nextShift = staff.Count(s => s.Status == "Active" && s.Availability == "Available" && s.Shift != "Night");

        return (totalStaff, onDuty, nextShift, unavailable);
    }

    public async Task<IEnumerable<ShiftCoverage>> GetCoverageAsync(string? shelter = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ShiftCoverages.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(shelter))
        {
            query = query.Where(c => c.Shelter.ToLower() == shelter.Trim().ToLower());
        }

        return await query.OrderBy(c => c.Date).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ShiftCoverage>> UpdateCoverageAsync(IEnumerable<ShiftCoverage> coverage, CancellationToken cancellationToken = default)
    {
        foreach (var item in coverage)
        {
            var existing = await _context.ShiftCoverages.FirstOrDefaultAsync(c => c.Id == item.Id || (c.Day == item.Day && c.Date == item.Date && c.Shelter == item.Shelter), cancellationToken);
            if (existing != null)
            {
                existing.Assigned = Math.Min(item.Capacity, Math.Max(0, item.Assigned));
                existing.Capacity = item.Capacity;
                _context.ShiftCoverages.Update(existing);
            }
            else
            {
                await _context.ShiftCoverages.AddAsync(item, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetCoverageAsync(coverage.FirstOrDefault()?.Shelter, cancellationToken);
    }
}
