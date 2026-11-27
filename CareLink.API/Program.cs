using System.Text;
using CareLink.API.Authorization;
using CareLink.Application;
using CareLink.Application.Common;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Infrastructure;
using CareLink.Infrastructure.Data;
using CareLink.Infrastructure.Repositories;
using CareLink.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// 1. Configure Database Connection (SQL Server for ASP.NET Core Identity)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=CareLinkIdentityDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Configure JWT Bearer Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CareLinkSecureEmergencyKeyForDevEnvironment2026";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CareLinkAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CareLinkClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var userManager = context.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();
                if (userManager != null)
                {
                    var dbUser = await userManager.FindByIdAsync(userId);
                    if (dbUser != null &&
                        (!dbUser.IsActive ||
                         dbUser.RequiresPasswordSetup ||
                         !string.Equals(dbUser.AccountStatus, AccountStatuses.Active, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Fail("Account is not active or is pending administrator approval.");
                    }
                }
            }
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = ApiResponse<object>.FailureResult(
                "Authentication required. Please sign in.");
            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var result = ApiResponse<object>.FailureResult(
                "Access denied. Your assigned role does not have permission to access this resource.");
            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// 4. Register Application & Infrastructure Clean Architecture layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure Email settings and Gemini AI settings
builder.Services.AddHttpClient();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.AddSingleton<IGeminiService, GeminiService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IIntegrationHealthService, IntegrationHealthService>();

// AI Recommendations with Gemini (database-backed repository)
builder.Services.AddScoped<IAiRecommendationRepository, AiRecommendationRepository>();
builder.Services.AddScoped<IAiRecommendationService, AiRecommendationService>();

// CareLink AI Assistant Chatbot (Gemini-powered for Citizen & Volunteer roles)
builder.Services.AddScoped<IChatbotRepository, ChatbotRepository>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();

builder.Services.AddSingleton<ISecurityEventService, SecurityEventService>();
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
builder.Services.AddSingleton<IOrganizationDocumentStorageService, OrganizationDocumentStorageService>();
builder.Services.AddSingleton<IVerificationService, VerificationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// 5. Register Resource Manager & Citizen Services (Oshadi)
builder.Services.AddSingleton<IAlertRepository, InMemoryAlertRepository>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddSingleton<IAllocationRepository, InMemoryAllocationRepository>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddSingleton<IAuditLogRepository, InMemoryAuditLogRepository>();
builder.Services.AddSingleton<IDispatchRepository, InMemoryDispatchRepository>();
builder.Services.AddScoped<IDispatchService, DispatchService>();
builder.Services.AddSingleton<IInventoryRepository, InMemoryInventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddSingleton<IProcurementRepository, InMemoryProcurementRepository>();
builder.Services.AddScoped<IProcurementService, ProcurementService>();
builder.Services.AddSingleton<IReportRepository, InMemoryReportRepository>();
builder.Services.AddSingleton<IResourceDashboardRepository, InMemoryResourceDashboardRepository>();
builder.Services.AddScoped<IResourceDashboardService, ResourceDashboardService>();
builder.Services.AddSingleton<IResourceRequestRepository, InMemoryResourceRequestRepository>();
builder.Services.AddScoped<IResourceRequestService, ResourceRequestService>();
builder.Services.AddSingleton<CareLink.Application.Interfaces.ISettingsRepository, InMemorySettingsRepository>();
builder.Services.AddSingleton<IStockTransferRepository, InMemoryStockTransferRepository>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddSingleton<ISupplierRepository, InMemorySupplierRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddSingleton<IWarehouseRepository, InMemoryWarehouseRepository>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddSingleton<IFeedbackRepository, InMemoryFeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddSingleton<ICitizenHomeRepository, InMemoryCitizenHomeRepository>();
builder.Services.AddScoped<ICitizenHomeService, CitizenHomeService>();
builder.Services.AddSingleton<IMessageRepository, InMemoryMessageRepository>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddSingleton<IDocumentRepository, InMemoryDocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddSingleton<IHouseholdRepository, InMemoryHouseholdRepository>();
builder.Services.AddScoped<IHouseholdService, HouseholdService>();
builder.Services.AddSingleton<ICitizenNotificationRepository, InMemoryCitizenNotificationRepository>();
builder.Services.AddScoped<ICitizenNotificationService, CitizenNotificationService>();
builder.Services.AddSingleton<ICitizenProfileRepository, InMemoryCitizenProfileRepository>();
builder.Services.AddScoped<ICitizenProfileService, CitizenProfileService>();
builder.Services.AddSingleton<ICitizenRequestRepository, InMemoryCitizenRequestRepository>();
builder.Services.AddScoped<ICitizenRequestService, CitizenRequestService>();
builder.Services.AddSingleton<ISafetyUpdateRepository, InMemorySafetyUpdateRepository>();
builder.Services.AddScoped<ISafetyUpdateService, SafetyUpdateService>();
builder.Services.AddSingleton<CareLink.Application.Interfaces.IShelterRepository, InMemoryShelterRepository>();

// 5b. Register Case Manager, Volunteer Coordinator & Shelter Reports Services (CareLink.Backend)
builder.Services.AddSingleton<CareLink.Backend.Services.ICaseManagementService, CareLink.Backend.Services.CaseManagementService>();
builder.Services.AddSingleton<CareLink.Backend.Services.ICaseReportsService, CareLink.Backend.Services.CaseReportsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.ICaseVerificationService, CareLink.Backend.Services.CaseVerificationService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IEscalatedCasesService, CareLink.Backend.Services.EscalatedCasesService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IMyCasesService, CareLink.Backend.Services.MyCasesService>();
builder.Services.AddSingleton<CareLink.Backend.Services.INotificationsService, CareLink.Backend.Services.NotificationsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IDuplicatesService, CareLink.Backend.Services.DuplicatesService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IPriorityQueueService, CareLink.Backend.Services.PriorityQueueService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IRequestHistoryService, CareLink.Backend.Services.RequestHistoryService>();
builder.Services.AddSingleton<CareLink.Backend.Services.ICaseManagerSettingsService, CareLink.Backend.Services.CaseManagerSettingsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IUnassignedCasesService, CareLink.Backend.Services.UnassignedCasesService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IActivityTrackingService, CareLink.Backend.Services.ActivityTrackingService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IAIVolunteerMatchingService, CareLink.Backend.Services.AIVolunteerMatchingService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IAssignmentsService, CareLink.Backend.Services.AssignmentsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.ICommunicationsService, CareLink.Backend.Services.CommunicationsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IVolunteerNotificationsService, CareLink.Backend.Services.VolunteerNotificationsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IVolunteerVerificationsService, CareLink.Backend.Services.VolunteerVerificationsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IVolunteerReportsService, CareLink.Backend.Services.VolunteerReportsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IShelterReportsService, CareLink.Backend.Services.ShelterReportsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IVolunteerSettingsService, CareLink.Backend.Services.VolunteerSettingsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.ISkillsAvailabilityService, CareLink.Backend.Services.SkillsAvailabilityService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IVolunteerTasksService, CareLink.Backend.Services.VolunteerTasksService>();
builder.Services.AddSingleton<CareLink.Backend.Services.ITrainingCertificationsService, CareLink.Backend.Services.TrainingCertificationsService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IVolunteerDashboardService, CareLink.Backend.Services.VolunteerDashboardService>();
builder.Services.AddSingleton<CareLink.Backend.Services.IVolunteerDirectoryService, CareLink.Backend.Services.VolunteerDirectoryService>();

// 6. Register Controllers & OpenAPI Documentation
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 7. Configure CORS for React frontend (Vite & standard dev ports)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CareLinkFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return uri.Host is "localhost" or "127.0.0.1" or "::1";
                }
                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// 8. Seed Database and Initial Records on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    // Seed Identity users and roles
    try
    {
        logger.LogInformation("Starting Identity database initialization and seeding...");
        await DbInitializer.SeedAsync(services);
        logger.LogInformation("Identity database initialized and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not seed ApplicationDbContext (Identity). Continuing startup.");
    }

    // Seed CareLinkDbContext operational entities
    try
    {
        logger.LogInformation("Starting CareLinkDbContext data seeding...");
        var careLinkDb = services.GetRequiredService<CareLinkDbContext>();
        await DataSeeder.SeedAsync(careLinkDb);
        logger.LogInformation("CareLinkDbContext seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during CareLinkDbContext seeding.");
    }
}

// 9. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("CareLinkFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health Check Endpoint
app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        application = "CareLink Humanitarian Crisis API",
        version = "1.0.0",
        timestamp = DateTime.UtcNow
    });
});

app.Run();

public partial class Program { }
