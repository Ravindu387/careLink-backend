namespace CareLink.Core.Constants;

public static class AccountStatuses
{
    public const string Active = "Active";
    public const string PendingPasswordSetup = "PendingPasswordSetup";
    public const string PendingApproval = "PendingApproval";
    public const string AdditionalInfoRequested = "AdditionalInfoRequested";
    public const string Rejected = "Rejected";
    public const string Disabled = "Disabled";

    public static readonly IReadOnlyList<string> All =
    [
        Active,
        PendingPasswordSetup,
        PendingApproval,
        AdditionalInfoRequested,
        Rejected,
        Disabled
    ];

    public static bool IsValid(string? status) =>
        !string.IsNullOrWhiteSpace(status) &&
        All.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase);
}
