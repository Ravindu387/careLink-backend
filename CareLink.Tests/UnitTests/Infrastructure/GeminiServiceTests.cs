using System.Net;
using System.Text.Json;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AiRecommendations;
using CareLink.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace CareLink.Tests.UnitTests.Infrastructure;

public class GeminiServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly GeminiOptions _options;
    private readonly Mock<IConfiguration> _configMock;

    public GeminiServiceTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var httpClient = new HttpClient(_httpHandlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient("Gemini")).Returns(httpClient);

        _options = new GeminiOptions
        {
            ApiKey = "test-fake-gemini-key",
            Model = "gemini-1.5-flash",
            TimeoutSeconds = 5,
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta"
        };

        _configMock = new Mock<IConfiguration>();
    }

    private GeminiService CreateService(GeminiOptions? customOptions = null, string? configApiKey = null)
    {
        var opts = Options.Create(customOptions ?? _options);
        if (configApiKey != null)
        {
            _configMock.Setup(c => c["Gemini:ApiKey"]).Returns(configApiKey);
        }

        return new GeminiService(
            _httpClientFactoryMock.Object,
            opts,
            _configMock.Object,
            NullLogger<GeminiService>.Instance);
    }

    [Fact]
    public void IsConfigured_WhenApiKeyPresent_ReturnsTrue()
    {
        var service = CreateService();
        Assert.True(service.IsConfigured);
        Assert.Equal("gemini-1.5-flash", service.ModelName);
    }

    [Fact]
    public void IsConfigured_WhenApiKeyEmpty_ReturnsFalse()
    {
        var emptyOptions = new GeminiOptions { ApiKey = "" };
        var service = CreateService(emptyOptions);
        Assert.False(service.IsConfigured);
    }

    [Fact]
    public async Task GenerateContentAsync_WhenUnconfigured_ThrowsInvalidOperationException()
    {
        var emptyOptions = new GeminiOptions { ApiKey = "" };
        var service = CreateService(emptyOptions);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateContentAsync("Test prompt"));
    }

    [Fact]
    public async Task GenerateContentAsync_SuccessfulResponse_ParsesCandidateText()
    {
        // Arrange
        var geminiResponseJson = @"{
            ""candidates"": [
                {
                    ""content"": {
                        ""parts"": [
                            { ""text"": ""This is valid AI generated response text."" }
                        ]
                    }
                }
            ]
        }";

        SetupHttpResponse(HttpStatusCode.OK, geminiResponseJson);
        var service = CreateService();

        // Act
        var result = await service.GenerateContentAsync("What is the flood status?");

        // Assert
        Assert.Equal("This is valid AI generated response text.", result);
    }

    [Fact]
    public async Task GenerateStructuredContentAsync_StripsCodeFencesAndDeserializesJson()
    {
        // Arrange: Gemini often wraps JSON in ```json ... ``` code fences
        var fencedJson = @"```json
{
    ""summary"": ""Fenced JSON test"",
    ""recommendedPriority"": ""Critical"",
    ""confidence"": 96,
    ""explanation"": ""Code fence stripping works."",
    ""recommendedAction"": ""Deploy rescue team"",
    ""immediatePriorities"": [""Priority 1""],
    ""risks"": [""Risk 1""],
    ""assumptions"": [""Assumption 1""],
    ""missingInformation"": [""Info 1""]
}
```";

        var geminiEnvelope = $@"{{
            ""candidates"": [
                {{
                    ""content"": {{
                        ""parts"": [
                            {{ ""text"": {JsonSerializer.Serialize(fencedJson)} }}
                        ]
                    }}
                }}
            ]
        }}";

        SetupHttpResponse(HttpStatusCode.OK, geminiEnvelope);
        var service = CreateService();

        // Act
        var result = await service.GenerateStructuredContentAsync<GeminiAnalysisResponse>("Analyze situation");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fenced JSON test", result.Summary);
        Assert.Equal("Critical", result.RecommendedPriority);
        Assert.Equal(96, result.Confidence);
        Assert.Contains("Priority 1", result.ImmediatePriorities);
    }

    [Fact]
    public async Task GenerateContentAsync_ApiReturnsRateLimit429_ThrowsInvalidOperationExceptionWithDetails()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.TooManyRequests, "{\"error\": {\"message\": \"Rate limit quota exceeded\"}}");
        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateContentAsync("Prompt"));

        Assert.Contains("rate limit exceeded", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateContentAsync_ApiReturnsServerError500_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Internal Server Error");
        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateContentAsync("Prompt"));

        Assert.Contains("service error", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateContentAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GenerateContentAsync("Prompt", cancellationToken: cts.Token));
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}
