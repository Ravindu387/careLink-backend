using CareLink.Application;
using CareLink.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Clean Architecture layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("CareLinkFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                var uri = new Uri(origin);
                return uri.Host == "localhost" || uri.Host == "127.0.0.1";
            })
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
