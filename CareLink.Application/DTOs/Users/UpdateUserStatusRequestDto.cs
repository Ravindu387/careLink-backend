namespace CareLink.Application.DTOs.Users;

public class UpdateUserStatusRequestDto
{
    public bool IsActive { get; set; }
    public string? Status { get; set; }
}
