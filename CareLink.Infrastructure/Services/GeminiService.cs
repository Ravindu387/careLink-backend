using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CareLink.Application.Common;
using CareLink.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareLink.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiService(
        IHttpClientFactory httpClientFactory,
        IOptions<GeminiOptions> options,
        IConfiguration configuration,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Gemini");
        _options = options.Value;
        _logger = logger;

        // Resolve API key in order: IOptions -> Configuration ("Gemini:ApiKey") -> Environment ("Gemini__ApiKey" or "GEMINI_API_KEY")
        _apiKey = !string.IsNullOrWhiteSpace(_options.ApiKey)
            ? _options.ApiKey
            : configuration["Gemini:ApiKey"]
              ?? configuration["Gemini__ApiKey"]
              ?? Environment.GetEnvironmentVariable("Gemini__ApiKey")
              ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
              ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. GeminiService will operate in fallback mode.");
        }
        else
        {
            _logger.LogInformation("GeminiService initialized with model: {Model}", _options.Model);
        }
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public string ModelName => _options.Model;

    public async Task<string> GenerateContentAsync(
        string prompt,
        string? systemInstruction = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "Google Gemini API key is not configured. Please configure 'Gemini:ApiKey' in appsettings.json or via the 'Gemini__ApiKey' environment variable.");
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(10, _options.TimeoutSeconds)));

        var url = $"{_options.BaseUrl.TrimEnd('/')}/models/{_options.Model}:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            systemInstruction = !string.IsNullOrWhiteSpace(systemInstruction)
                ? new
                {
                    parts = new[]
                    {
                        new { text = systemInstruction }
                    }
                }
                : null,
            generationConfig = new
            {
                temperature = 0.2,
                topK = 40,
                topP = 0.95,
                maxOutputTokens = 4096,
                responseMimeType = "application/json"
            }
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.PostAsync(url, content, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cts.Token);
                HandleGeminiError(response.StatusCode, errorBody);
            }

            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var candidateContent) &&
                candidateContent.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textElem))
            {
                return textElem.GetString() ?? string.Empty;
            }

            _logger.LogWarning("Gemini API returned response without text candidate parts.");
            return string.Empty;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Gemini API call timed out after {Seconds} seconds.", _options.TimeoutSeconds);
            throw new TimeoutException($"Gemini AI generation timed out after {_options.TimeoutSeconds} seconds.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error communicating with Gemini API.");
            throw new InvalidOperationException("Failed to reach Google Gemini API. Please verify network connectivity.", ex);
        }
    }

    public async Task<T?> GenerateStructuredContentAsync<T>(
        string prompt,
        string? systemInstruction = null,
        CancellationToken cancellationToken = default)
    {
        var rawText = await GenerateContentAsync(prompt, systemInstruction, cancellationToken);
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return default;
        }

        // Clean any markdown code fence wrappers (e.g. ```json ... ```)
        var cleaned = rawText.Trim();
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[7..];
        }
        else if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned[3..];
        }

        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned[..^3];
        }

        cleaned = cleaned.Trim();

        try
        {
            return JsonSerializer.Deserialize<T>(cleaned, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Gemini structured JSON into {Type}. Raw: {Text}", typeof(T).Name, cleaned);
            throw new InvalidOperationException($"The AI model returned an invalid structured JSON response for {typeof(T).Name}.", ex);
        }
    }

    private void HandleGeminiError(HttpStatusCode statusCode, string errorBody)
    {
        _logger.LogError("Gemini API error (Status: {StatusCode}): {Error}", statusCode, errorBody);

        switch (statusCode)
        {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                throw new InvalidOperationException("The configured Google Gemini API key is invalid or unauthorized.");

            case HttpStatusCode.TooManyRequests:
                throw new InvalidOperationException("Gemini API rate limit or quota exceeded. Please try again in a few moments.");

            case HttpStatusCode.BadRequest:
                throw new InvalidOperationException("The request payload was rejected by Google Gemini API.");

            case HttpStatusCode.InternalServerError:
            case HttpStatusCode.ServiceUnavailable:
                throw new InvalidOperationException("Google Gemini AI service is temporarily unavailable. Please retry shortly.");

            default:
                throw new InvalidOperationException($"Google Gemini API request failed with status code {statusCode}.");
        }
    }
}
