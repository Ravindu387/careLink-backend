using System.Text.Json.Serialization;

namespace CareLink.Application.DTOs;

public class AccommodationRequestCountsDto
{
    [JsonPropertyName("All requests")]
    public int AllRequests { get; set; }

    [JsonPropertyName("Pending")]
    public int Pending { get; set; }

    [JsonPropertyName("Under Review")]
    public int UnderReview { get; set; }

    [JsonPropertyName("Approved")]
    public int Approved { get; set; }

    [JsonPropertyName("Assigned")]
    public int Assigned { get; set; }
}
