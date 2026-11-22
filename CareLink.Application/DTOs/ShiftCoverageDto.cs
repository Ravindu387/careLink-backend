namespace CareLink.Application.DTOs;

public class ShiftCoverageDto
{
    public int Id { get; set; }
    public string Day { get; set; } = string.Empty;
    public int Date { get; set; }
    public int Assigned { get; set; }
    public int Capacity { get; set; }
    public string Shelter { get; set; } = string.Empty;
}
