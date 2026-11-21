using CareLink.Core.Interfaces;
using CareLink.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CareLink.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
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
        services.AddSingleton<INotificationRepository, InMemoryNotificationRepository>();
        services.AddSingleton<IOperationDashboardRepository, InMemoryOperationDashboardRepository>();
        services.AddSingleton<IOrganisationProfileRepository, InMemoryOrganisationProfileRepository>();
        services.AddSingleton<IPartnerDashboardRepository, InMemoryPartnerDashboardRepository>();
        services.AddSingleton<IPartnerDocumentRepository, InMemoryPartnerDocumentRepository>();
        services.AddSingleton<IPartnerOrganisationRepository, InMemoryPartnerOrganisationRepository>();
        services.AddSingleton<IPartnerReportRepository, InMemoryPartnerReportRepository>();
        services.AddSingleton<IResourceSharingRepository, InMemoryResourceSharingRepository>();
        services.AddSingleton<ITeamMemberRepository, InMemoryTeamMemberRepository>();
        services.AddSingleton<IPriorityQueueRepository, InMemoryPriorityQueueRepository>();
        services.AddSingleton<IResourceRepository, InMemoryResourceRepository>();
        services.AddSingleton<ISettingsRepository, InMemorySettingsRepository>();
        services.AddSingleton<IShelterRepository, InMemoryShelterRepository>();
        services.AddSingleton<ISituationReportRepository, InMemorySituationReportRepository>();
        return services;
    }
}
