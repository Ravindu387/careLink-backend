namespace CareLink.Application.Common;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>
    /// Google Gemini API key. Can be supplied via appsettings.json, environment variable Gemini__ApiKey or GEMINI_API_KEY.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model name, e.g. "gemini-1.5-flash", "gemini-1.5-pro", or "gemini-2.5-flash".
    /// </summary>
    public string Model { get; set; } = "gemini-1.5-flash";

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Gemini API base URL.
    /// </summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
}
