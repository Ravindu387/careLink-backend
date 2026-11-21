using CareLink.Application;
using CareLink.Infrastructure;
using CareLink.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Register controllers and OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Application & Infrastructure layer services (Clean Architecture)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("CareLinkFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Seed initial database records
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CareLinkDbContext>();
    await DataSeeder.SeedAsync(dbContext);
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use CORS
app.UseCors("CareLinkFrontend");

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
