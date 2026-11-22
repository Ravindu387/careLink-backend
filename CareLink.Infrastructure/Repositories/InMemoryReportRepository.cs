namespace CareLink.Infrastructure.Repositories;

using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryReportRepository : IReportRepository
{
    private readonly List<ReportItem> _reports = [];
    private readonly List<ReportSchedule> _schedules = [];
    private readonly Lock _lock = new();

    public InMemoryReportRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<ReportItem>> GetAllReportsAsync(ReportFilterParams? filters = null)
    {
        lock (_lock)
        {
            IEnumerable<ReportItem> query = _reports.OrderByDescending(r => r.CreatedAt).ToList();

            if (filters != null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var s = filters.Search.Trim().ToLowerInvariant();
                    query = query.Where(r =>
                        r.Name.ToLowerInvariant().Contains(s) ||
                        r.Category.ToLowerInvariant().Contains(s) ||
                        r.Format.ToLowerInvariant().Contains(s)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filters.Category) &&
                    !filters.Category.Equals("All reports", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Category.Equals(filters.Category, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filters.Format) &&
                    !filters.Format.Equals("All formats", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Format.Equals(filters.Format, StringComparison.OrdinalIgnoreCase));
                }
            }

            return Task.FromResult(query);
        }
    }

    public Task<ReportItem?> GetReportByIdAsync(int id)
    {
        lock (_lock)
        {
            var item = _reports.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(item);
        }
    }

    public Task<ReportItem> AddReportAsync(ReportItem report)
    {
        lock (_lock)
        {
            if (report.Id == 0)
            {
                report.Id = _reports.Count > 0 ? _reports.Max(r => r.Id) + 1 : 1;
            }

            report.CreatedAt = DateTime.UtcNow;
            _reports.Insert(0, report);
            return Task.FromResult(report);
        }
    }

    public Task<bool> DeleteReportAsync(int id)
    {
        lock (_lock)
        {
            var index = _reports.FindIndex(r => r.Id == id);
            if (index == -1)
            {
                return Task.FromResult(false);
            }
            _reports.RemoveAt(index);
            return Task.FromResult(true);
        }
    }

    public Task<IEnumerable<ReportSchedule>> GetAllSchedulesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<ReportSchedule>>(_schedules.ToList());
        }
    }

    public Task<ReportSchedule?> GetScheduleByIdAsync(int id)
    {
        lock (_lock)
        {
            var item = _schedules.FirstOrDefault(s => s.Id == id);
            return Task.FromResult(item);
        }
    }

    public Task<ReportSchedule> AddScheduleAsync(ReportSchedule schedule)
    {
        lock (_lock)
        {
            schedule.Id = _schedules.Count > 0 ? _schedules.Max(s => s.Id) + 1 : 1;
            schedule.CreatedAt = DateTime.UtcNow;
            _schedules.Insert(0, schedule);
            return Task.FromResult(schedule);
        }
    }

    public Task<ReportSchedule?> ToggleScheduleStatusAsync(int id)
    {
        lock (_lock)
        {
            var schedule = _schedules.FirstOrDefault(s => s.Id == id);
            if (schedule == null)
            {
                return Task.FromResult<ReportSchedule?>(null);
            }
            schedule.IsActive = !schedule.IsActive;
            return Task.FromResult<ReportSchedule?>(schedule);
        }
    }

    public Task<bool> DeleteScheduleAsync(int id)
    {
        lock (_lock)
        {
            var index = _schedules.FindIndex(s => s.Id == id);
            if (index == -1)
            {
                return Task.FromResult(false);
            }
            _schedules.RemoveAt(index);
            return Task.FromResult(true);
        }
    }

    public Task<AnalyticsDashboardDto> GetAnalyticsDashboardAsync(AnalyticsQueryParams? query = null)
    {
        var selectedPeriod = string.IsNullOrWhiteSpace(query?.Period) ? "Last 30 days" : query.Period.Trim();
        var selectedOperation = string.IsNullOrWhiteSpace(query?.Operation) ? "Monsoon Floods 2025" : query.Operation.Trim();
        var selectedWarehouse = string.IsNullOrWhiteSpace(query?.Warehouse) ? "All warehouses" : query.Warehouse.Trim();

        var kpi = new AnalyticsKpiDto();
        var weeks = new List<WeeklyTrendDto>();

        if (selectedPeriod.Equals("Last 7 days", StringComparison.OrdinalIgnoreCase))
        {
            kpi.RequestsFulfilled = "95.1%";
            kpi.RequestsFulfilledTrend = "+3.4% vs previous week";
            kpi.AverageFulfilmentTime = "2h 45m";
            kpi.AverageFulfilmentTimeTrend = "-35 min improvement";
            kpi.InventoryTurnover = "5.1×";
            kpi.StockLossRate = "0.31%";

            weeks.AddRange([
                new WeeklyTrendDto { Label = "Mon", Received = 18, Fulfilled = 17 },
                new WeeklyTrendDto { Label = "Tue", Received = 24, Fulfilled = 23 },
                new WeeklyTrendDto { Label = "Wed", Received = 31, Fulfilled = 29 },
                new WeeklyTrendDto { Label = "Thu", Received = 28, Fulfilled = 27 },
                new WeeklyTrendDto { Label = "Fri", Received = 35, Fulfilled = 34 }
            ]);
        }
        else if (selectedPeriod.Equals("Last 90 days", StringComparison.OrdinalIgnoreCase))
        {
            kpi.RequestsFulfilled = "91.2%";
            kpi.RequestsFulfilledTrend = "+1.0% vs previous quarter";
            kpi.AverageFulfilmentTime = "3h 40m";
            kpi.AverageFulfilmentTimeTrend = "-10 min improvement";
            kpi.InventoryTurnover = "4.2×";
            kpi.StockLossRate = "0.48%";

            weeks.AddRange([
                new WeeklyTrendDto { Label = "Month 1", Received = 280, Fulfilled = 254 },
                new WeeklyTrendDto { Label = "Month 2", Received = 340, Fulfilled = 310 },
                new WeeklyTrendDto { Label = "Month 3", Received = 390, Fulfilled = 360 }
            ]);
        }
        else
        {
            // Default "Last 30 days" / "This year"
            kpi.RequestsFulfilled = "93.8%";
            kpi.RequestsFulfilledTrend = "+2.1% vs previous period";
            kpi.AverageFulfilmentTime = "3h 18m";
            kpi.AverageFulfilmentTimeTrend = "-24 min improvement";
            kpi.InventoryTurnover = "4.6×";
            kpi.StockLossRate = "0.42%";

            weeks.AddRange([
                new WeeklyTrendDto { Label = "Week 1", Received = 68, Fulfilled = 61 },
                new WeeklyTrendDto { Label = "Week 2", Received = 82, Fulfilled = 73 },
                new WeeklyTrendDto { Label = "Week 3", Received = 94, Fulfilled = 86 },
                new WeeklyTrendDto { Label = "Week 4", Received = 103, Fulfilled = 96 }
            ]);
        }

        var categories = new List<CategoryDistributionDto>
        {
            new() { Name = "Food & Water", Value = 82, Color = "bg-emerald-500" },
            new() { Name = "Shelter", Value = 68, Color = "bg-sky-600" },
            new() { Name = "Medical", Value = 24, Color = "bg-red-500" },
            new() { Name = "WASH", Value = 74, Color = "bg-teal-500" },
            new() { Name = "Transport & Fuel", Value = 36, Color = "bg-amber-500" },
            new() { Name = "Non-food Items", Value = 57, Color = "bg-blue-500" }
        };

        var dashboard = new AnalyticsDashboardDto
        {
            Kpis = kpi,
            WeeklyTrends = weeks,
            CategoryDistributions = categories,
            Period = selectedPeriod,
            Operation = selectedOperation,
            Warehouse = selectedWarehouse
        };

        return Task.FromResult(dashboard);
    }

    private void SeedInitialData()
    {
        _reports.AddRange([
            new ReportItem
            {
                Id = 1,
                Name = "Resource Fulfilment Report",
                Category = "Fulfilment",
                Period = "Last 30 days",
                Generated = "Today, 09:30 AM",
                Format = "PDF",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new ReportItem
            {
                Id = 2,
                Name = "Inventory Stock Summary",
                Category = "Inventory",
                Period = "Last 30 days",
                Generated = "Yesterday, 04:15 PM",
                Format = "CSV",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new ReportItem
            {
                Id = 3,
                Name = "Warehouse Performance",
                Category = "Warehouse",
                Period = "Last 7 days",
                Generated = "Yesterday, 11:20 AM",
                Format = "PDF",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-4)
            },
            new ReportItem
            {
                Id = 4,
                Name = "Supplier Delivery Analysis",
                Category = "Suppliers",
                Period = "Last 30 days",
                Generated = "22 Jun 2025",
                Format = "CSV",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new ReportItem
            {
                Id = 5,
                Name = "Stock Movement Report",
                Category = "Inventory",
                Period = "Last 90 days",
                Generated = "21 Jun 2025",
                Format = "PDF",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        ]);

        _schedules.Add(new ReportSchedule
        {
            Id = 1,
            Name = "Weekly Resource Report",
            Email = "operations@carelink.org",
            Frequency = "Weekly",
            Format = "PDF",
            Period = "Last 30 days",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        });
    }
}
