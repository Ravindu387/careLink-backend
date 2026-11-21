namespace CareLink.Application.DTOs.Users;

public class UserListQueryDto
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
    public string? Tab { get; set; }
    public string? Organization { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
