using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly CareLinkDbContext _context;

    public CampaignRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Campaign>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Campaign> Items, int TotalCount)> GetPagedAsync(
        CampaignQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Campaigns.AsNoTracking().AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Id.ToLower().Contains(search) ||
                c.Event.ToLower().Contains(search));
        }

        // Archive filtering
        if (parameters.IncludeArchived.HasValue)
        {
            if (parameters.IncludeArchived.Value)
            {
                query = query.Where(c => c.Status == "Archived");
            }
            else
            {
                query = query.Where(c => c.Status != "Archived");
            }
        }

        // Event filter
        if (!string.IsNullOrWhiteSpace(parameters.Event) &&
            !parameters.Event.Equals("All crisis events", StringComparison.OrdinalIgnoreCase))
        {
            var ev = parameters.Event.Trim().ToLower();
            query = query.Where(c => c.Event.ToLower() == ev);
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            var st = parameters.Status.Trim().ToLower();
            query = query.Where(c => c.Status.ToLower() == st);
        }

        // Target range
        if (parameters.MinTarget.HasValue)
        {
            query = query.Where(c => c.Target >= parameters.MinTarget.Value);
        }

        if (parameters.MaxTarget.HasValue)
        {
            query = query.Where(c => c.Target <= parameters.MaxTarget.Value);
        }

        // Progress percentage filter
        if (!string.IsNullOrWhiteSpace(parameters.Progress) &&
            !parameters.Progress.Equals("Any progress", StringComparison.OrdinalIgnoreCase))
        {
            var pr = parameters.Progress.Trim().ToLower();
            if (pr.Contains("below 50"))
            {
                query = query.Where(c => c.Target > 0 && (c.Raised / c.Target) < 0.5m);
            }
            else if (pr.Contains("50") && pr.Contains("75"))
            {
                query = query.Where(c => c.Target > 0 && (c.Raised / c.Target) >= 0.5m && (c.Raised / c.Target) <= 0.75m);
            }
            else if (pr.Contains("above 75"))
            {
                query = query.Where(c => c.Target > 0 && (c.Raised / c.Target) > 0.75m);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page.GetValueOrDefault(1);
        if (page < 1) page = 1;
        var pageSize = parameters.PageSize.GetValueOrDefault(10);
        if (pageSize < 1) pageSize = 10;

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Campaign?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Campaign> AddAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        await _context.Campaigns.AddAsync(campaign, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return campaign;
    }

    public async Task<Campaign?> UpdateAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        _context.Campaigns.Update(campaign);
        await _context.SaveChangesAsync(cancellationToken);
        return campaign;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.Campaigns.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(int ActiveCampaigns, decimal CombinedTarget, decimal TotalRaised, int EndingSoon)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var active = await _context.Campaigns
            .AsNoTracking()
            .Where(c => c.Status == "Active")
            .ToListAsync(cancellationToken);

        var activeCount = active.Count;
        var combinedTarget = active.Sum(c => c.Target);
        var totalRaised = active.Sum(c => c.Raised);

        var now = DateTime.UtcNow.Date;
        var fourteenDaysLater = now.AddDays(14);
        var endingSoon = active.Count(c => c.EndDate.Date >= now && c.EndDate.Date <= fourteenDaysLater);

        return (activeCount, combinedTarget, totalRaised, endingSoon);
    }
}
