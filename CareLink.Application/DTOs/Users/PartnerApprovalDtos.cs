namespace CareLink.Application.DTOs.Users;

public class ApprovePartnerRequestDto
{
    public string? Notes { get; set; }
}

public class RejectPartnerRequestDto
{
    public string? Reason { get; set; }
}
