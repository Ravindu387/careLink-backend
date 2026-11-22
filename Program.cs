using CareLink.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Controllers
builder.Services.AddControllers();

// Configure Services
builder.Services.AddSingleton<ICaseManagementService, CaseManagementService>();
builder.Services.AddSingleton<ICaseReportsService, CaseReportsService>();
builder.Services.AddSingleton<ICaseVerificationService, CaseVerificationService>();
builder.Services.AddSingleton<IEscalatedCasesService, EscalatedCasesService>();
builder.Services.AddSingleton<IMyCasesService, MyCasesService>();
builder.Services.AddSingleton<INotificationsService, NotificationsService>();
builder.Services.AddSingleton<IDuplicatesService, DuplicatesService>();
builder.Services.AddSingleton<IPriorityQueueService, PriorityQueueService>();
builder.Services.AddSingleton<IRequestHistoryService, RequestHistoryService>();
builder.Services.AddSingleton<ICaseManagerSettingsService, CaseManagerSettingsService>();
builder.Services.AddSingleton<IUnassignedCasesService, UnassignedCasesService>();
builder.Services.AddSingleton<IActivityTrackingService, ActivityTrackingService>();
builder.Services.AddSingleton<IAIVolunteerMatchingService, AIVolunteerMatchingService>();
builder.Services.AddSingleton<IAssignmentsService, AssignmentsService>();
builder.Services.AddSingleton<ICommunicationsService, CommunicationsService>();
builder.Services.AddSingleton<IVolunteerNotificationsService, VolunteerNotificationsService>();
builder.Services.AddSingleton<IVolunteerVerificationsService, VolunteerVerificationsService>();
builder.Services.AddSingleton<IVolunteerReportsService, VolunteerReportsService>();
builder.Services.AddSingleton<IShelterReportsService, ShelterReportsService>();
builder.Services.AddSingleton<IVolunteerSettingsService, VolunteerSettingsService>();
builder.Services.AddSingleton<ISkillsAvailabilityService, SkillsAvailabilityService>();
builder.Services.AddSingleton<IVolunteerTasksService, VolunteerTasksService>();
builder.Services.AddSingleton<ITrainingCertificationsService, TrainingCertificationsService>();
builder.Services.AddSingleton<IVolunteerDashboardService, VolunteerDashboardService>();
builder.Services.AddSingleton<IVolunteerDirectoryService, VolunteerDirectoryService>();

// Configure OpenAPI / Swagger
builder.Services.AddOpenApi();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "CareLink Backend API",
    status = "Online",
    version = "1.0.0",
    docs = "/openapi/v1.json"
}));

app.Run("http://localhost:5277");






