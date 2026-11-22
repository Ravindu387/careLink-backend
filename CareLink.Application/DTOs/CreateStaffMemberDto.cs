namespace CareLink.Application.DTOs;

public class CreateStaffMemberDto
{
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Shelter { get; set; } = string.Empty;
    public string Shift { get; set; } = "Morning";
    public string Availability { get; set; } = "Available";
    public string Status { get; set; } = "Active";
}
