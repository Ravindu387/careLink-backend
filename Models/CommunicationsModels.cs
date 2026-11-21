namespace CareLink.Backend.Models;

public class Broadcast
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int Delivered { get; set; }
    public string Time { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Channel { get; set; } = "In-app";
    public List<string> Channels { get; set; } = [];
    public string Status { get; set; } = "Delivered";
    public string Message { get; set; } = string.Empty;
    public string Sender { get; set; } = "Nadia Okafor (Coordinator)";
    public int FailedCount { get; set; } = 0;
}

public class AudienceOption
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Category { get; set; } = "General";
}

public class ChannelHealth
{
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = "Operational";
    public double SuccessRate { get; set; } = 99.5;
    public string Latency { get; set; } = "< 1s";
    public string Description { get; set; } = "Operational";
}

public class CommunicationsOverviewDto
{
    public int TotalBroadcasts { get; set; }
    public int TotalRecipientsDelivered { get; set; }
    public int ActiveVolunteers { get; set; }
    public double DeliverySuccessRate { get; set; }
    public List<ChannelHealth> ChannelHealth { get; set; } = [];
}

public class CreateBroadcastRequest
{
    public string Title { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Channels { get; set; } = [];
    public string? Sender { get; set; }
}

public class BroadcastQueryDto
{
    public string? Search { get; set; }
    public string? Channel { get; set; }
    public string? Status { get; set; }
    public string? Audience { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CommunicationDraftDto
{
    public string Id { get; set; } = "draft-default";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public List<string> Channels { get; set; } = [];
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class SaveDraftRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public List<string> Channels { get; set; } = [];
}
