namespace CareLink.Application.DTOs;

public class ResourceDistributionDto
{
    public string Id { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string DestinationShelter { get; set; } = string.Empty;
    public string DestinationShelterId { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string DistributedAt { get; set; } = string.Empty;
    public DateTime DistributedAtUtc { get; set; }
}
