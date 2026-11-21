using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class TransparencyReportRepository : ITransparencyReportRepository
{
    private readonly CareLinkDbContext _context;

    public TransparencyReportRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<TransparencyReport> Items, int TotalCount)> GetPagedAsync(
        TransparencyReportQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(parameters);
        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 12;

        var items = await query
            .OrderByDescending(r => r.ReportDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<TransparencyReport>> GetAllAsync(
        TransparencyReportQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(parameters);
        return await query
            .OrderByDescending(r => r.ReportDate)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TransparencyReport?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.TransparencyReports
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<TransparencyReport> AddAsync(TransparencyReport report, CancellationToken cancellationToken = default)
    {
        await _context.TransparencyReports.AddAsync(report, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<bool> UpdateAsync(TransparencyReport report, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TransparencyReports
            .FirstOrDefaultAsync(r => r.Id == report.Id, cancellationToken);

        if (existing == null) return false;

        if (!ReferenceEquals(existing, report))
        {
            _context.Entry(existing).CurrentValues.SetValues(report);
        }

        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> PublishAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TransparencyReports
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (existing == null) return false;

        existing.Status = "Published";
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TransparencyReports
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (existing == null) return false;

        _context.TransparencyReports.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<TransparencyReport> BuildQuery(TransparencyReportQueryParameters parameters)
    {
        var query = _context.TransparencyReports.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim().ToLower();
            query = query.Where(r =>
                r.Id.ToLower().Contains(term) ||
                r.Title.ToLower().Contains(term) ||
                r.Reference.ToLower().Contains(term) ||
                (r.Description != null && r.Description.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Type) &&
            !parameters.Type.Equals("All report types", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Type.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var type = parameters.Type.Trim().ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(type));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim().ToLower();
            query = query.Where(r => r.Status.ToLower() == status);
        }

        return query;
    }
}
