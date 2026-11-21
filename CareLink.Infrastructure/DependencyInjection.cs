using CareLink.Core.Interfaces;
using CareLink.Infrastructure.Data;
using CareLink.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareLink.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CareLinkDbContext>(options =>
        {
            options.UseInMemoryDatabase("CareLinkDb");
        });

        services.AddScoped<IAccommodationRequestRepository, AccommodationRequestRepository>();
        services.AddScoped<IShelterRepository, ShelterRepository>();
        services.AddScoped<IDisasterUpdateRepository, DisasterUpdateRepository>();
        services.AddScoped<IHealthSafetyRepository, HealthSafetyRepository>();
        services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IOccupancyRepository, OccupancyRepository>();
        services.AddScoped<IReportsRepository, ReportsRepository>();
        services.AddScoped<IResidentRepository, ResidentRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IRoomBedRepository, RoomBedRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IDonationRepository, DonationRepository>();
        services.AddScoped<IDonorRepository, DonorRepository>();
        services.AddScoped<IExpenditureRepository, ExpenditureRepository>();
        services.AddScoped<IFlaggedTransactionRepository, FlaggedTransactionRepository>();
        services.AddScoped<IFundAllocationRepository, FundAllocationRepository>();
        services.AddScoped<IFundingDashboardRepository, FundingDashboardRepository>();
        services.AddScoped<IFundingReportsRepository, FundingReportsRepository>();
        services.AddScoped<IFundingSettingsRepository, FundingSettingsRepository>();
        services.AddScoped<IResourceContributionRepository, ResourceContributionRepository>();
        services.AddScoped<ITransparencyReportRepository, TransparencyReportRepository>();

        return services;
    }
}
