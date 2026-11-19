using CareLink.Application.Common;
using CareLink.Application.DTOs.Dashboard;
using CareLink.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CareLink.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUserService _userService;
    private readonly IAssistanceRequestService _requestService;
    private readonly IOrganizationService _organizationService;
    private readonly IVerificationService _verificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly ISecurityEventService _securityEventService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IUserService userService,
        IAssistanceRequestService requestService,
        IOrganizationService organizationService,
        IVerificationService verificationService,
        IAuditLogService auditLogService,
        ISecurityEventService securityEventService,
        ILogger<DashboardService> logger)
    {
        _userService = userService;
        _requestService = requestService;
        _organizationService = organizationService;
        _verificationService = verificationService;
        _auditLogService = auditLogService;
        _securityEventService = securityEventService;
        _logger = logger;
    }

    public async Task<ApiResponse<DashboardDataDto>> GetDashboardDataAsync()
    {
        var summary = await GetSummaryAsync();
        var trend = await GetWeeklyTrendAsync();
        var activities = await GetRecentActivitiesAsync();
        var alerts = await GetActiveAlertsAsync();

        // Get urgent requests preview
        var requestsRes = await _requestService.GetRequestsAsync(new DTOs.AssistanceRequests.AssistanceRequestQueryDto
        {
            PageSize = 3
        });

        var urgentRequests = requestsRes.Data?.Items.Select(r => new DashboardRequestPreviewDto
        {
            Id = r.Id,
            Description = $"{r.Category} · {r.TimeAgo}",
            Category = r.Category,
            Location = r.Location,
            Priority = $"{r.ReportedPriority} · {r.Confidence}",
            Status = r.Status,
            AffectedPeople = r.AffectedPeople,
            Contact = r.Contact,
            Critical = r.ReportedPriority.Equals("Critical", StringComparison.OrdinalIgnoreCase),
            TimeAgo = r.TimeAgo
        }).ToList() ?? new List<DashboardRequestPreviewDto>();

        var data = new DashboardDataDto
        {
            Summary = summary.Data ?? new DashboardSummaryDto(),
            WeeklyTrend = trend.Data ?? new List<DashboardChartPointDto>(),
            UrgentRequests = urgentRequests,
            RecentActivities = activities.Data ?? new List<DashboardActivityItemDto>(),
            ActiveAlerts = alerts.Data ?? new List<DashboardAlertItemDto>()
        };

        return ApiResponse<DashboardDataDto>.SuccessResult(data);
    }

    public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync()
    {
        var userStats = await _userService.GetUserStatsAsync();
        var reqStats = await _requestService.GetStatsAsync();
        var orgStats = await _organizationService.GetStatsAsync();
        var verStats = await _verificationService.GetStatsAsync();

        var uData = userStats.Data;
        var rData = reqStats.Data;
        var oData = orgStats.Data;
        var vData = verStats.Data;

        var roleDist = new List<RoleDistributionDto>
        {
            new() { Role = "EmergencyCoordinator", Count = 3 },
            new() { Role = "PartnerOfficer", Count = 2 },
            new() { Role = "Volunteer", Count = 1 },
            new() { Role = "Citizen", Count = Math.Max(0, (uData?.TotalUsers ?? 7) - 6) }
        };

        var summary = new DashboardSummaryDto
        {
            OpenRequestsCount = rData?.AwaitingReviewCount ?? 1284,
            OpenRequestsTodayDelta = 86,
            CriticalRequestsCount = rData?.CriticalCount ?? 47,
            OverdueCriticalCount = 12,
            PeopleInSheltersCount = 8642,
            ShelterCapacityPercentage = 76,
            ResourcesAvailablePercentage = 68,
            ResourceShortagesCount = 3,
            TotalRegisteredUsers = uData?.TotalUsers ?? 7,
            ActiveUsersCount = uData?.ActiveUsers ?? 5,
            PendingVerificationCount = vData?.TotalPending ?? 14,
            ActiveVolunteersCount = 128,
            PendingPartnerApprovalsCount = oData?.PendingOrganizations ?? 2,
            ActiveCrisisName = "Monsoon Floods 2025",
            ActiveCrisisStatus = "Active Emergency",
            RoleDistribution = roleDist
        };

        return ApiResponse<DashboardSummaryDto>.SuccessResult(summary);
    }

    public Task<ApiResponse<IReadOnlyList<DashboardChartPointDto>>> GetWeeklyTrendAsync()
    {
        IReadOnlyList<DashboardChartPointDto> trend = new List<DashboardChartPointDto>
        {
            new() { Day = "Mon", Received = 165, Fulfilled = 112 },
            new() { Day = "Tue", Received = 205, Fulfilled = 135 },
            new() { Day = "Wed", Received = 145, Fulfilled = 155 },
            new() { Day = "Thu", Received = 260, Fulfilled = 195 },
            new() { Day = "Fri", Received = 225, Fulfilled = 210 },
            new() { Day = "Sat", Received = 290, Fulfilled = 235 },
            new() { Day = "Today", Received = 195, Fulfilled = 155 }
        };

        return Task.FromResult(ApiResponse<IReadOnlyList<DashboardChartPointDto>>.SuccessResult(trend));
    }

    public async Task<ApiResponse<IReadOnlyList<DashboardActivityItemDto>>> GetRecentActivitiesAsync()
    {
        var auditRes = await _auditLogService.GetAuditLogsAsync(new DTOs.AuditLogs.AuditLogQueryDto { PageSize = 4 });
        var items = new List<DashboardActivityItemDto>();

        if (auditRes.Data?.Items != null)
        {
            foreach (var log in auditRes.Data.Items)
            {
                var icon = log.Action.Contains("role", StringComparison.OrdinalIgnoreCase) ? "ShieldCheck" :
                           log.Action.Contains("account", StringComparison.OrdinalIgnoreCase) ? "AlertTriangle" :
                           log.Action.Contains("dispatch", StringComparison.OrdinalIgnoreCase) ? "Truck" : "Clock";

                var color = log.Action.Contains("disabled", StringComparison.OrdinalIgnoreCase) ? "red" :
                            log.Action.Contains("role", StringComparison.OrdinalIgnoreCase) ? "blue" : "orange";

                items.Add(new DashboardActivityItemDto
                {
                    Id = log.Id,
                    Title = log.Action,
                    Description = $"{log.Record} · {log.Actor}",
                    TimeAgo = log.Timestamp,
                    Icon = icon,
                    Color = color,
                    Timestamp = log.CreatedAt
                });
            }
        }

        if (items.Count == 0)
        {
            items.Add(new DashboardActivityItemDto
            {
                Id = "ACT-1",
                Title = "Dispatch DEP-104 departed",
                Description = "2,400 L water · Eastbank",
                TimeAgo = "4 min ago",
                Icon = "Truck",
                Color = "blue",
                Timestamp = DateTime.UtcNow.AddMinutes(-4)
            });
            items.Add(new DashboardActivityItemDto
            {
                Id = "ACT-2",
                Title = "Critical request escalated",
                Description = "REQ-2841 · North Ward",
                TimeAgo = "12 min ago",
                Icon = "AlertTriangle",
                Color = "red",
                Timestamp = DateTime.UtcNow.AddMinutes(-12)
            });
        }

        return ApiResponse<IReadOnlyList<DashboardActivityItemDto>>.SuccessResult(items);
    }

    public async Task<ApiResponse<IReadOnlyList<DashboardAlertItemDto>>> GetActiveAlertsAsync()
    {
        var eventsRes = await _securityEventService.GetEventsAsync(new DTOs.SecurityEvents.SecurityEventQueryDto
        {
            Severity = "Critical",
            PageSize = 2
        });

        var alerts = new List<DashboardAlertItemDto>();
        if (eventsRes.Data?.Items != null)
        {
            foreach (var ev in eventsRes.Data.Items)
            {
                alerts.Add(new DashboardAlertItemDto
                {
                    Id = ev.Id,
                    Title = ev.Type,
                    Description = ev.Description ?? ev.Type,
                    Severity = ev.Severity,
                    TimeAgo = ev.Timestamp,
                    Timestamp = ev.CreatedAt
                });
            }
        }

        if (alerts.Count == 0)
        {
            alerts.Add(new DashboardAlertItemDto
            {
                Id = "ALT-1",
                Title = "Supply Shortage Warning",
                Description = "Medical trauma kits in Sector 4 depot dropped below 15% threshold.",
                Severity = "High",
                TimeAgo = "15 min ago",
                Timestamp = DateTime.UtcNow.AddMinutes(-15)
            });
        }

        return ApiResponse<IReadOnlyList<DashboardAlertItemDto>>.SuccessResult(alerts);
    }
}
