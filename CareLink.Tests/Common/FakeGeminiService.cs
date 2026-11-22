using System.Text.Json;
using CareLink.Application.Interfaces;

namespace CareLink.Tests.Common;

public enum FakeGeminiBehavior
{
    Success,
    InvalidJson,
    EmptyResponse,
    Timeout,
    ApiError,
    RateLimit,
    Cancellation,
    Unconfigured
}

public class FakeGeminiService : IGeminiService
{
    public FakeGeminiBehavior Behavior { get; set; } = FakeGeminiBehavior.Success;
    public string ModelName { get; set; } = "gemini-1.5-flash-test";
    public bool IsConfigured => Behavior != FakeGeminiBehavior.Unconfigured;

    public int CallCount { get; private set; }
    public string? LastPrompt { get; private set; }
    public string? LastSystemInstruction { get; private set; }
    public object? CustomStructuredResult { get; set; }
    public string? CustomRawResult { get; set; }

    public Task<string> GenerateContentAsync(
        string prompt,
        string? systemInstruction = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastPrompt = prompt;
        LastSystemInstruction = systemInstruction;

        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        switch (Behavior)
        {
            case FakeGeminiBehavior.Unconfigured:
                throw new InvalidOperationException("Google Gemini API key is not configured.");
            case FakeGeminiBehavior.Timeout:
                throw new TimeoutException("Gemini AI generation timed out after 60 seconds.");
            case FakeGeminiBehavior.ApiError:
                throw new HttpRequestException("Gemini API error: 500 Internal Server Error");
            case FakeGeminiBehavior.RateLimit:
                throw new HttpRequestException("Gemini API rate limit exceeded (429 Too Many Requests).");
            case FakeGeminiBehavior.EmptyResponse:
                return Task.FromResult(string.Empty);
            case FakeGeminiBehavior.InvalidJson:
                return Task.FromResult("This is malformed non-JSON text from Gemini.");
            case FakeGeminiBehavior.Cancellation:
                throw new OperationCanceledException();
            case FakeGeminiBehavior.Success:
            default:
                return Task.FromResult(CustomRawResult ?? "{\"summary\": \"AI test summary\"}");
        }
    }

    public Task<T?> GenerateStructuredContentAsync<T>(
        string prompt,
        string? systemInstruction = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastPrompt = prompt;
        LastSystemInstruction = systemInstruction;

        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        switch (Behavior)
        {
            case FakeGeminiBehavior.Unconfigured:
                throw new InvalidOperationException("Google Gemini API key is not configured.");
            case FakeGeminiBehavior.Timeout:
                throw new TimeoutException("Gemini AI generation timed out after 60 seconds.");
            case FakeGeminiBehavior.ApiError:
                throw new HttpRequestException("Gemini API error: 500 Internal Server Error");
            case FakeGeminiBehavior.RateLimit:
                throw new HttpRequestException("Gemini API rate limit exceeded (429 Too Many Requests).");
            case FakeGeminiBehavior.EmptyResponse:
                return Task.FromResult<T?>(default);
            case FakeGeminiBehavior.InvalidJson:
                throw new JsonException("Invalid JSON format from Gemini response.");
            case FakeGeminiBehavior.Cancellation:
                throw new OperationCanceledException();
            case FakeGeminiBehavior.Success:
            default:
                if (CustomStructuredResult is T typedResult)
                {
                    return Task.FromResult<T?>(typedResult);
                }
                if (!string.IsNullOrWhiteSpace(CustomRawResult))
                {
                    var deserialized = JsonSerializer.Deserialize<T>(CustomRawResult, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return Task.FromResult(deserialized);
                }
                return Task.FromResult<T?>(default);
        }
    }
}
