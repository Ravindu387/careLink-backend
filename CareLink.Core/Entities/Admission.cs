namespace CareLink.Core.Entities;

public class Admission
{
    public string Id { get; set; } = string.Empty;
    public string Resident { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public int People { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
