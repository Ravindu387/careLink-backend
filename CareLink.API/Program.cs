using System.Text;
using CareLink.API.Authorization;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Infrastructure.Data;
using CareLink.Infrastructure.Repositories;
using CareLink.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Database Connection (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured.");
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
    options.RequireHttpsMetadata = false; // Set to true in production
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
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = CareLink.Application.Common.ApiResponse<object>.FailureResult(
                "Authentication required. Please sign in.");
            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var result = CareLink.Application.Common.ApiResponse<object>.FailureResult(
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

// 4. Register Application & Infrastructure Services (Dependency Injection)
builder.Services.AddHttpClient();
builder.Services.Configure<CareLink.Application.Common.EmailSettings>(
    builder.Configuration.GetSection(CareLink.Application.Common.EmailSettings.SectionName));
builder.Services.Configure<CareLink.Application.Common.GeminiOptions>(
    builder.Configuration.GetSection(CareLink.Application.Common.GeminiOptions.SectionName));
builder.Services.AddSingleton<IGeminiService, GeminiService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IIntegrationHealthService, IntegrationHealthService>();
builder.Services.AddScoped<IAiRecommendationRepository, AiRecommendationRepository>();
builder.Services.AddScoped<IAiRecommendationService, AiRecommendationService>();
builder.Services.AddSingleton<ISecurityEventService, SecurityEventService>();
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
builder.Services.AddSingleton<IVerificationService, VerificationService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddSingleton<IAssistanceRequestService, AssistanceRequestService>();
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// 5. Register Controllers & OpenAPI Documentation
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 6. Configure CORS for React frontend (Vite port 5173 by default)
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

// 7. Seed Database and Initial Roles/Users on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting database initialization and seeding...");
        await DbInitializer.SeedAsync(services);
        logger.LogInformation("Database initialized and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}

// 8. Configure HTTP Request Pipeline
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
