using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;

    public StaffService(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<IEnumerable<StaffMemberDto>> GetAllStaffAsync(CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetAllAsync(cancellationToken);
        return staff.Select(MapToDto);
    }

    public async Task<PagedResultDto<StaffMemberDto>> GetPagedStaffAsync(StaffQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _staffRepository.GetPagedAsync(parameters, cancellationToken);

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        return new PagedResultDto<StaffMemberDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<StaffMemberDto?> GetStaffByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var member = await _staffRepository.GetByIdAsync(id, cancellationToken);
        if (member == null) return null;
        return MapToDto(member);
    }

    public async Task<StaffMemberDto> CreateStaffAsync(CreateStaffMemberDto dto, CancellationToken cancellationToken = default)
    {
        var all = await _staffRepository.GetAllAsync(cancellationToken);
        var maxIdNum = all
            .Select(s => s.Id.Replace("ST-", ""))
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();

        var id = $"ST-{(maxIdNum + 1):D3}";

        var member = new StaffMember
        {
            Id = id,
            Name = dto.Name.Trim(),
            Designation = dto.Designation.Trim(),
            Contact = dto.Contact.Trim(),
            Email = dto.Email.Trim(),
            Shelter = dto.Shelter.Trim(),
            Shift = string.IsNullOrWhiteSpace(dto.Shift) ? "Morning" : dto.Shift.Trim(),
            Availability = string.IsNullOrWhiteSpace(dto.Availability) ? "Available" : dto.Availability.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _staffRepository.AddAsync(member, cancellationToken);
        return MapToDto(created);
    }

    public async Task<StaffMemberDto?> UpdateStaffAsync(string id, UpdateStaffMemberDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _staffRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) existing.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Designation)) existing.Designation = dto.Designation.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Contact)) existing.Contact = dto.Contact.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Email)) existing.Email = dto.Email.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Shelter)) existing.Shelter = dto.Shelter.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Shift)) existing.Shift = dto.Shift.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Availability)) existing.Availability = dto.Availability.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _staffRepository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteStaffAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _staffRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<StaffSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var (totalStaff, onDuty, nextShift, unavailable) = await _staffRepository.GetSummaryMetricsAsync(cancellationToken);

        return new StaffSummaryDto
        {
            TotalStaff = totalStaff,
            OnDuty = onDuty,
            NextShift = nextShift,
            Unavailable = unavailable
        };
    }

    public async Task<IEnumerable<ShiftCoverageDto>> GetCoverageAsync(string? shelter = null, CancellationToken cancellationToken = default)
    {
        var coverage = await _staffRepository.GetCoverageAsync(shelter, cancellationToken);
        return coverage.Select(c => new ShiftCoverageDto
        {
            Id = c.Id,
            Day = c.Day,
            Date = c.Date,
            Assigned = c.Assigned,
            Capacity = c.Capacity,
            Shelter = c.Shelter
        });
    }

    public async Task<IEnumerable<ShiftCoverageDto>> UpdateCoverageAsync(IEnumerable<ShiftCoverageDto> coverage, CancellationToken cancellationToken = default)
    {
        var entities = coverage.Select(c => new ShiftCoverage
        {
            Id = c.Id,
            Day = c.Day,
            Date = c.Date,
            Assigned = c.Assigned,
            Capacity = c.Capacity,
            Shelter = string.IsNullOrWhiteSpace(c.Shelter) ? "Riverton Central" : c.Shelter
        });

        var updated = await _staffRepository.UpdateCoverageAsync(entities, cancellationToken);
        return updated.Select(c => new ShiftCoverageDto
        {
            Id = c.Id,
            Day = c.Day,
            Date = c.Date,
            Assigned = c.Assigned,
            Capacity = c.Capacity,
            Shelter = c.Shelter
        });
    }

    private static StaffMemberDto MapToDto(StaffMember s)
    {
        return new StaffMemberDto
        {
            Id = s.Id,
            Name = s.Name,
            Designation = s.Designation,
            Contact = s.Contact,
            Email = s.Email,
            Shelter = s.Shelter,
            Shift = s.Shift,
            Availability = s.Availability,
            Status = s.Status
        };
    }
}
