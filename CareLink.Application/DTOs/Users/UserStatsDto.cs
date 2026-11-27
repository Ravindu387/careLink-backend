namespace CareLink.Application.DTOs.Users;

public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PendingUsers { get; set; }
    public int PendingSetupUsers { get; set; }
    public int RejectedUsers { get; set; }
    public int DisabledUsers { get; set; }
    public int PrivilegedUsers { get; set; }
}
