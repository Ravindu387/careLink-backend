namespace CareLink.Core.Entities;

public class ShiftCoverage
{
    public int Id { get; set; }
    public string Day { get; set; } = string.Empty;
    public int Date { get; set; }
    public int Assigned { get; set; }
    public int Capacity { get; set; } = 24;
    public string Shelter { get; set; } = "Riverton Central";
}
