using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CareLink.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AI & Operational / Emergency Services (HEAD)
        services.AddSingleton<IAIInferenceEngine, AIInferenceEngine>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<IAssistanceRequestService, AssistanceRequestService>();
        services.AddScoped<IAssistanceReferralService, AssistanceReferralService>();
        services.AddScoped<IAuditTimelineService, AuditTimelineService>();
        services.AddScoped<ICollaborationRequestService, CollaborationRequestService>();
        services.AddScoped<ICrisisEventService, CrisisEventService>();
        services.AddScoped<IEmergencySidebarService, EmergencySidebarService>();
        services.AddScoped<IEmergencyTopbarService, EmergencyTopbarService>();
        services.AddScoped<IFieldActivityService, FieldActivityService>();
        services.AddScoped<ILiveOperationsMapService, LiveOperationsMapService>();
        services.AddScoped<IOperationDashboardService, OperationDashboardService>();
        services.AddScoped<IOrganisationProfileService, OrganisationProfileService>();
        services.AddScoped<IPartnerDashboardService, PartnerDashboardService>();
        services.AddScoped<IPartnerDocumentService, PartnerDocumentService>();
        services.AddScoped<IPartnerOrganisationService, PartnerOrganisationService>();
        services.AddScoped<IPartnerReportService, PartnerReportService>();
        services.AddScoped<IResourceSharingService, ResourceSharingService>();
        services.AddScoped<ITeamMemberService, TeamMemberService>();
        services.AddScoped<IPriorityQueueService, PriorityQueueService>();
        services.AddScoped<ISituationReportService, SituationReportService>();

        // Unified Core Services
        services.AddScoped<IShelterService, ShelterService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISettingsService, SettingsService>();

        // Shelter & Funding Services (Prabhani)
        services.AddScoped<IAccommodationRequestService, AccommodationRequestService>();
        services.AddScoped<IDisasterUpdateService, DisasterUpdateService>();
        services.AddScoped<IHealthSafetyService, HealthSafetyService>();
        services.AddScoped<IMaintenanceRequestService, MaintenanceRequestService>();
        services.AddScoped<IOccupancyService, OccupancyService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<IResidentService, ResidentService>();
        services.AddScoped<IRoomBedService, RoomBedService>();
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
