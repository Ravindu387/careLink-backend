namespace CareLink.Application.DTOs;

public class WarehouseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Capacity { get; set; }
    public double Utilisation { get; set; }
    public int StockLines { get; set; }
    public int Staff { get; set; }
    public string Status { get; set; } = "Operational";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Capacity { get; set; }
    public int Staff { get; set; }
    public double? Utilisation { get; set; }
    public int? StockLines { get; set; }
    public string? Status { get; set; }
}

public class UpdateWarehouseStatusRequest
{
    public string? Status { get; set; }
    public double? Utilisation { get; set; }
    public int? Staff { get; set; }
    public int? StockLines { get; set; }
}

public class WarehouseSummaryDto
{
    public int OperationalSites { get; set; }
    public double AverageUtilisation { get; set; }
    public int ReceivingToday { get; set; }
    public int CapacityAlerts { get; set; }
    public int TotalCount { get; set; }
}
