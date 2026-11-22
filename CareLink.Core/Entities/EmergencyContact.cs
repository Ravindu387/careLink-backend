namespace CareLink.Core.Entities;

public class EmergencyContact
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Department { get; set; }
}
