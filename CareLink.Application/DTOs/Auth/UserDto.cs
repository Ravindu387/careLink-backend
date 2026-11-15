namespace CareLink.Application.DTOs.Auth;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? NIC { get; set; }
    public string? Address { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
