using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register application & infrastructure services
builder.Services.AddSingleton<IAlertRepository, InMemoryAlertRepository>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddSingleton<IAllocationRepository, InMemoryAllocationRepository>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddSingleton<IAuditLogRepository, InMemoryAuditLogRepository>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddSingleton<IDispatchRepository, InMemoryDispatchRepository>();
builder.Services.AddScoped<IDispatchService, DispatchService>();
builder.Services.AddSingleton<IInventoryRepository, InMemoryInventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddSingleton<IProcurementRepository, InMemoryProcurementRepository>();
builder.Services.AddScoped<IProcurementService, ProcurementService>();
builder.Services.AddSingleton<IReportRepository, InMemoryReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddSingleton<IResourceDashboardRepository, InMemoryResourceDashboardRepository>();
builder.Services.AddScoped<IResourceDashboardService, ResourceDashboardService>();
builder.Services.AddSingleton<IResourceRequestRepository, InMemoryResourceRequestRepository>();
builder.Services.AddScoped<IResourceRequestService, ResourceRequestService>();
builder.Services.AddSingleton<ISettingsRepository, InMemorySettingsRepository>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
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
builder.Services.AddSingleton<IShelterRepository, InMemoryShelterRepository>();
builder.Services.AddScoped<IShelterService, ShelterService>();

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("CareLinkFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use CORS
app.UseCors("CareLinkFrontend");

// Temporarily disabled for local HTTP development.
// Configure HTTPS before production deployment.
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        application = "CareLink API",
        message = "Backend is running successfully."
    });
});

app.Run();
