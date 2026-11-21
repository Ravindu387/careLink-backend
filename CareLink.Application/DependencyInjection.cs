using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CareLink.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAccommodationRequestService, AccommodationRequestService>();
        services.AddScoped<IShelterService, ShelterService>();
        services.AddScoped<IDisasterUpdateService, DisasterUpdateService>();
        services.AddScoped<IHealthSafetyService, HealthSafetyService>();
        services.AddScoped<IMaintenanceRequestService, MaintenanceRequestService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOccupancyService, OccupancyService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<IResidentService, ResidentService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IRoomBedService, RoomBedService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<IDonationService, DonationService>();
        services.AddScoped<IDonorService, DonorService>();
        services.AddScoped<IExpenditureService, ExpenditureService>();
        services.AddScoped<IFlaggedTransactionService, FlaggedTransactionService>();
        services.AddScoped<IFundAllocationService, FundAllocationService>();
        services.AddScoped<IFundingDashboardService, FundingDashboardService>();
        services.AddScoped<IFundingReportsService, FundingReportsService>();
        services.AddScoped<IFundingSettingsService, FundingSettingsService>();
        services.AddScoped<IResourceContributionService, ResourceContributionService>();
        services.AddScoped<ITransparencyReportService, TransparencyReportService>();
        return services;
    }
}
