namespace CareLink.Application.Interfaces;

public interface IGeminiService
{
    /// <summary>
    /// Indicates whether a valid Google Gemini API Key has been provided in backend configuration.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// The active model name (e.g. gemini-1.5-flash).
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Generates text or JSON content from the Gemini API.
    /// </summary>
    Task<string> GenerateContentAsync(
        string prompt,
        string? systemInstruction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates structured strongly-typed JSON content parsed into the specified type T.
    /// </summary>
    Task<T?> GenerateStructuredContentAsync<T>(
        string prompt,
        string? systemInstruction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a conversational chat response supporting multi-turn history and system instructions.
    /// </summary>
    Task<string> GenerateChatMessageAsync(
        IEnumerable<(string Role, string Content)> conversationHistory,
        string userMessage,
        string? systemInstruction = null,
        CancellationToken cancellationToken = default);
}
