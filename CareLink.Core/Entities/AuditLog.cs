namespace CareLink.Core.Entities;

public class AuditLog
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string? ActorId { get; set; }
    public string Record { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string Type { get; set; } = "System"; // User, Organisation, System, Integration, Role, Shelter, Report, Security

    private string? _actionType;
    public string ActionType
    {
        get => _actionType ?? Action;
        set => _actionType = value;
    }

    private string? _recordType;
    public string RecordType
    {
        get => _recordType ?? Type;
        set => _recordType = value;
    }

    private string? _timestamp;
    public string Timestamp
    {
        get => _timestamp ?? CreatedAt.ToString("dd MMM · HH:mm");
        set => _timestamp = value;
    }

    private string? _date;
    public string Date
    {
        get => _date ?? CreatedAt.ToString("yyyy-MM-dd");
        set => _date = value;
    }

    public string Environment { get; set; } = "Production";
    public string? IpAddress { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
