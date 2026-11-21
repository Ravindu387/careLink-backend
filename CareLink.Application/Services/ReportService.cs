namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAllocationRepository _allocationRepository;

    public ReportService(
        IReportRepository repository,
        IInventoryRepository inventoryRepository,
        IAllocationRepository allocationRepository)
    {
        _repository = repository;
        _inventoryRepository = inventoryRepository;
        _allocationRepository = allocationRepository;
    }

    public async Task<ApiResponse<List<ReportItemDto>>> GetReportsAsync(ReportFilterParams? filters = null)
    {
        var items = await _repository.GetAllReportsAsync(filters);
        var dtos = items.Select(MapToReportDto).ToList();
        return ApiResponse<List<ReportItemDto>>.SuccessResponse(dtos, "Reports retrieved successfully.");
    }

    public async Task<ApiResponse<ReportItemDto>> GetReportByIdAsync(int id)
    {
        var report = await _repository.GetReportByIdAsync(id);
        if (report == null)
        {
            return ApiResponse<ReportItemDto>.ErrorResponse($"Report with ID '{id}' was not found.");
        }
        return ApiResponse<ReportItemDto>.SuccessResponse(MapToReportDto(report));
    }

    public async Task<ApiResponse<ReportItemDto>> GenerateReportAsync(GenerateReportRequest request)
    {
        var now = DateTime.UtcNow;
        var generatedTime = $"Today, {now:hh:mm tt}";

        var item = new ReportItem
        {
            Name = string.IsNullOrWhiteSpace(request.Name) ? "Operational Analytics Report" : request.Name.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "Fulfilment" : request.Category.Trim(),
            Period = string.IsNullOrWhiteSpace(request.Period) ? "Last 30 days" : request.Period.Trim(),
            Generated = generatedTime,
            Format = string.IsNullOrWhiteSpace(request.Format) ? "CSV" : request.Format.Trim().ToUpperInvariant(),
            CreatedAt = now
        };

        var created = await _repository.AddReportAsync(item);
        return ApiResponse<ReportItemDto>.SuccessResponse(MapToReportDto(created), "Report generated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteReportAsync(int id)
    {
        var deleted = await _repository.DeleteReportAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Report with ID '{id}' was not found.");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Report deleted successfully.");
    }

    public async Task<ApiResponse<List<ReportScheduleDto>>> GetSchedulesAsync()
    {
        var schedules = await _repository.GetAllSchedulesAsync();
        var dtos = schedules.Select(MapToScheduleDto).ToList();
        return ApiResponse<List<ReportScheduleDto>>.SuccessResponse(dtos, "Schedules retrieved successfully.");
    }

    public async Task<ApiResponse<ReportScheduleDto>> CreateScheduleAsync(CreateReportScheduleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<ReportScheduleDto>.ErrorResponse("Report schedule name is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiResponse<ReportScheduleDto>.ErrorResponse("Email address is required.");
        }

        var schedule = new ReportSchedule
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Frequency = string.IsNullOrWhiteSpace(request.Frequency) ? "Weekly" : request.Frequency.Trim(),
            Format = string.IsNullOrWhiteSpace(request.Format) ? "PDF" : request.Format.Trim().ToUpperInvariant(),
            Period = string.IsNullOrWhiteSpace(request.Period) ? "Last 30 days" : request.Period.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddScheduleAsync(schedule);
        return ApiResponse<ReportScheduleDto>.SuccessResponse(MapToScheduleDto(created), "Report schedule saved successfully.");
    }

    public async Task<ApiResponse<ReportScheduleDto>> ToggleScheduleStatusAsync(int id)
    {
        var toggled = await _repository.ToggleScheduleStatusAsync(id);
        if (toggled == null)
        {
            return ApiResponse<ReportScheduleDto>.ErrorResponse($"Report schedule with ID '{id}' was not found.");
        }
        return ApiResponse<ReportScheduleDto>.SuccessResponse(MapToScheduleDto(toggled), "Schedule status updated.");
    }

    public async Task<ApiResponse<bool>> DeleteScheduleAsync(int id)
    {
        var deleted = await _repository.DeleteScheduleAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Report schedule with ID '{id}' was not found.");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Schedule deleted successfully.");
    }

    public async Task<ApiResponse<AnalyticsDashboardDto>> GetAnalyticsDashboardAsync(AnalyticsQueryParams? query = null)
    {
        var dashboard = await _repository.GetAnalyticsDashboardAsync(query);

        try
        {
            // Augment with real inventory data from IInventoryRepository
            var inventoryItems = (await _inventoryRepository.GetAllAsync(new InventoryFilterParams
            {
                Warehouse = query?.Warehouse != "All warehouses" ? query?.Warehouse : null
            })).ToList();

            if (inventoryItems.Count > 0)
            {
                var totalAvailable = inventoryItems.Sum(i => i.Available);
                if (totalAvailable > 0)
                {
                    var catGroups = inventoryItems
                        .GroupBy(i => i.Category)
                        .Select(g => new
                        {
                            Category = g.Key,
                            Units = g.Sum(i => i.Available),
                            Percentage = (int)Math.Max(5, Math.Min(95, Math.Round((double)g.Sum(i => i.Available) / totalAvailable * 100)))
                        })
                        .OrderByDescending(x => x.Units)
                        .Take(6)
                        .ToList();

                    var colors = new[] { "bg-emerald-500", "bg-sky-600", "bg-red-500", "bg-teal-500", "bg-amber-500", "bg-blue-500" };
                    var dynamicCategories = new List<CategoryDistributionDto>();
                    for (int i = 0; i < catGroups.Count; i++)
                    {
                        dynamicCategories.Add(new CategoryDistributionDto
                        {
                            Name = catGroups[i].Category,
                            Value = catGroups[i].Percentage,
                            Color = colors[i % colors.Length]
                        });
                    }

                    if (dynamicCategories.Count > 0)
                    {
                        dashboard.CategoryDistributions = dynamicCategories;
                    }
                }
            }

            // Augment with real allocation metrics from IAllocationRepository
            var allAllocations = (await _allocationRepository.GetAllAsync()).ToList();
            var allocations = (query?.Warehouse != null && query.Warehouse != "All warehouses")
                ? allAllocations.Where(a => a.Warehouse.Equals(query.Warehouse, StringComparison.OrdinalIgnoreCase) || a.Source.Equals(query.Warehouse, StringComparison.OrdinalIgnoreCase)).ToList()
                : allAllocations;

            if (allocations.Count > 0)
            {
                var completed = allocations.Count(a => a.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
                var percentage = (double)completed / allocations.Count * 100;
                dashboard.Kpis.RequestsFulfilled = $"{percentage:F1}%";
                dashboard.Kpis.RequestsFulfilledTrend = $"{allocations.Count} total allocations evaluated";
            }
        }
        catch
        {
            // Fallback to baseline calculation if related services are quiet
        }

        return ApiResponse<AnalyticsDashboardDto>.SuccessResponse(dashboard, "Analytics dashboard retrieved successfully.");
    }

    public async Task<(byte[] Bytes, string ContentType, string FileName)> ExportReportFileAsync(int? id, string? format = "csv", ReportFilterParams? filters = null)
    {
        var isPdf = string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase);

        if (id.HasValue && id.Value > 0)
        {
            var report = await _repository.GetReportByIdAsync(id.Value);
            var headers = new[] { "Field", "Detail" };
            var rows = new List<string[]>
            {
                new[] { "Report ID", report?.Id.ToString() ?? id.Value.ToString() },
                new[] { "Report Name", report?.Name ?? "Report" },
                new[] { "Category", report?.Category ?? "General" },
                new[] { "Reporting Period", report?.Period ?? "N/A" },
                new[] { "Generated At", report?.Generated ?? DateTime.UtcNow.ToString("g") },
                new[] { "File Format", report?.Format ?? (isPdf ? "PDF" : "CSV") }
            };

            var title = report?.Name ?? "CareLink Operational Report";
            var subtitle = $"Category: {report?.Category ?? "General"} | Period: {report?.Period ?? "N/A"}";

            if (isPdf)
            {
                var bytes = ReportDocumentGenerator.GeneratePdf(title, subtitle, rows, headers);
                return (bytes, "application/pdf", $"carelink-report-{id.Value}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
            }
            else
            {
                var bytes = ReportDocumentGenerator.GenerateCsv(rows, headers);
                return (bytes, "text/csv; charset=utf-8", $"carelink-report-{id.Value}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }
        }
        else
        {
            var reports = (await _repository.GetAllReportsAsync(filters)).ToList();
            var headers = new[] { "Report ID", "Report Name", "Category", "Reporting Period", "Generated", "Format" };
            var rows = reports.Select(r => new[]
            {
                r.Id.ToString(),
                r.Name,
                r.Category,
                r.Period,
                r.Generated,
                r.Format
            }).ToList();

            var title = "CareLink Operational Reports Summary";
            var subtitle = $"Filtered Reports: {reports.Count} | Exported on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";

            if (isPdf)
            {
                var bytes = ReportDocumentGenerator.GeneratePdf(title, subtitle, rows, headers);
                return (bytes, "application/pdf", $"carelink-reports-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
            }
            else
            {
                var bytes = ReportDocumentGenerator.GenerateCsv(rows, headers);
                return (bytes, "text/csv; charset=utf-8", $"carelink-reports-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }
        }
    }

    private static ReportItemDto MapToReportDto(ReportItem entity)
    {
        return new ReportItemDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Category = entity.Category,
            Period = entity.Period,
            Generated = entity.Generated,
            Format = entity.Format,
            DownloadUrl = entity.DownloadUrl,
            CreatedAt = entity.CreatedAt
        };
    }

    private static ReportScheduleDto MapToScheduleDto(ReportSchedule entity)
    {
        return new ReportScheduleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email,
            Frequency = entity.Frequency,
            Format = entity.Format,
            Period = entity.Period,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }
}
