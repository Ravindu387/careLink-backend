namespace CareLink.Core.Entities;

public class OperationPoint
{
    public string Id { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string District { get; set; } = "North Ward";
    public string Location { get; set; } = string.Empty;
    public string Type { get; set; } = "Request"; // Request, Shelter, Warehouse, Team
    public string Priority { get; set; } = "High"; // Critical, High, Medium, Low
    public int People { get; set; } = 1;
    public int Confidence { get; set; } = 90;
    public string NearestShelter { get; set; } = "1.5 km";
    public string AssignedTeam { get; set; } = "Unassigned";
    public string Status { get; set; } = "Active"; // Active, Dispatched, Operational, In Transit, Resolved
    public string Time { get; set; } = "Recently";
    public double X { get; set; } = 50;
    public double Y { get; set; } = 50;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public int? Capacity { get; set; }
    public int? CurrentOccupancy { get; set; }
    public string? SuppliesStatus { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
