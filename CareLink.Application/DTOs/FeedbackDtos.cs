namespace CareLink.Application.DTOs;

public class FeedbackDto
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool Anonymous { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
    public string Status { get; set; } = "Submitted";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateFeedbackRequest
{
    public string Category { get; set; } = "Using the citizen portal";
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool Anonymous { get; set; }
    public string? SubmittedBy { get; set; }
}

public class UpdateFeedbackStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class FeedbackSummaryDto
{
    public int TotalCount { get; set; }
    public double AverageRating { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int LowRatingCount { get; set; }
}
