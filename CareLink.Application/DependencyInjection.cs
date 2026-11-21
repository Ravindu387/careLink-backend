using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CareLink.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IAIInferenceEngine, AIInferenceEngine>();
        services.AddScoped<IAIRecommendationService, AIRecommendationService>();
        services.AddScoped<IAssistanceRequestService, AssistanceRequestService>();
        services.AddScoped<IAssistanceReferralService, AssistanceReferralService>();
        services.AddScoped<IAuditTimelineService, AuditTimelineService>();
        services.AddScoped<ICollaborationRequestService, CollaborationRequestService>();
        services.AddScoped<ICrisisEventService, CrisisEventService>();
        services.AddScoped<IEmergencySidebarService, EmergencySidebarService>();
        services.AddScoped<IEmergencyTopbarService, EmergencyTopbarService>();
        services.AddScoped<IFieldActivityService, FieldActivityService>();
        services.AddScoped<ILiveOperationsMapService, LiveOperationsMapService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOperationDashboardService, OperationDashboardService>();
        services.AddScoped<IOrganisationProfileService, OrganisationProfileService>();
        services.AddScoped<IPartnerDashboardService, PartnerDashboardService>();
        services.AddScoped<IPartnerDocumentService, PartnerDocumentService>();
        services.AddScoped<IPartnerOrganisationService, PartnerOrganisationService>();
        services.AddScoped<IPartnerReportService, PartnerReportService>();
        services.AddScoped<IResourceSharingService, ResourceSharingService>();
        services.AddScoped<ITeamMemberService, TeamMemberService>();
        services.AddScoped<IPriorityQueueService, PriorityQueueService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IShelterService, ShelterService>();
        services.AddScoped<ISituationReportService, SituationReportService>();
        return services;
    }
}
