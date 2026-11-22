using CareLink.Core.Interfaces;
using CareLink.Infrastructure.Data;
using CareLink.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareLink.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // 1. EF Core In-Memory Database for Humanitarian Operations
        services.AddDbContext<CareLinkDbContext>(options =>
        {
            options.UseInMemoryDatabase("CareLinkDb");
        });

        // 2. EF Core database repositories (Unified & Prabhani)
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

        // 3. Coordinator & Partner Domain Repositories (HEAD)
        services.AddSingleton<IRecommendationRepository, InMemoryRecommendationRepository>();
        services.AddSingleton<IAssistanceRequestRepository, InMemoryAssistanceRequestRepository>();
        services.AddSingleton<IAssistanceReferralRepository, InMemoryAssistanceReferralRepository>();
        services.AddSingleton<IAuditTimelineRepository, InMemoryAuditTimelineRepository>();
        services.AddSingleton<ICollaborationRequestRepository, InMemoryCollaborationRequestRepository>();
        services.AddSingleton<ICrisisEventRepository, InMemoryCrisisEventRepository>();
        services.AddSingleton<IEmergencySidebarRepository, InMemoryEmergencySidebarRepository>();
        services.AddSingleton<IEmergencyTopbarRepository, InMemoryEmergencyTopbarRepository>();
        services.AddSingleton<IFieldActivityRepository, InMemoryFieldActivityRepository>();
        services.AddSingleton<ILiveOperationsMapRepository, InMemoryLiveOperationsMapRepository>();
        services.AddSingleton<IOperationDashboardRepository, InMemoryOperationDashboardRepository>();
        services.AddSingleton<IOrganisationProfileRepository, InMemoryOrganisationProfileRepository>();
        services.AddSingleton<IPartnerDashboardRepository, InMemoryPartnerDashboardRepository>();
        services.AddSingleton<IPartnerDocumentRepository, InMemoryPartnerDocumentRepository>();
        services.AddSingleton<IPartnerOrganisationRepository, InMemoryPartnerOrganisationRepository>();
        services.AddSingleton<IPartnerReportRepository, InMemoryPartnerReportRepository>();
        services.AddSingleton<IResourceSharingRepository, InMemoryResourceSharingRepository>();
        services.AddSingleton<ITeamMemberRepository, InMemoryTeamMemberRepository>();
        services.AddSingleton<IPriorityQueueRepository, InMemoryPriorityQueueRepository>();
        services.AddSingleton<ISituationReportRepository, InMemorySituationReportRepository>();

        return services;
    }
}
