using CareLink.Application.Common;
using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CareLink.Tests.IntegrationTests;

[Trait("Category", "RealGemini")]
public class RealGeminiIntegrationTests
{
    private readonly string? _apiKey;

    public RealGeminiIntegrationTests()
    {
        // Read key only from environment without logging or exposing it
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                  ?? Environment.GetEnvironmentVariable("Gemini__ApiKey");
    }

    [Fact]
    public async Task RealGemini_LiveCall_WhenConfigured_GeneratesValidResponse()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            // Explicitly skipped when no API key configured in test environment
            return;
        }

        // Arrange
        var options = Options.Create(new GeminiOptions
        {
            ApiKey = _apiKey,
            Model = "gemini-1.5-flash",
            TimeoutSeconds = 30
        });

        var config = new ConfigurationBuilder().Build();
        var httpClientFactory = new SimpleHttpClientFactory();

        var service = new GeminiService(
            httpClientFactory,
            options,
            config,
            NullLogger<GeminiService>.Instance);

        // Act
        var result = await service.GenerateStructuredContentAsync<GeminiAnalysisResponse>(
            "Give a 1-sentence situation assessment for monsoon flooding. Return valid JSON matching schema.",
            "You are CareLink AI. Return only clean JSON.",
            default);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Summary));
        Assert.Contains(result.RecommendedPriority, new[] { "Critical", "High", "Medium", "Low" });
        Assert.True(result.Confidence >= 60 && result.Confidence <= 99);
    }

    private class SimpleHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}
