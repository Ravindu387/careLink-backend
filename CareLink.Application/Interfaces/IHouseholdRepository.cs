namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface IHouseholdRepository
{
    Task<HouseholdProfile> GetProfileAsync(string? citizenName = null);
    Task<HouseholdProfile> UpdateAddressAsync(string address, string? ward = null, string? city = null, string? citizenName = null);
    Task<HouseholdProfile> UpdateEmergencyContactAsync(string contactName, string contactPhone, string? relationship = null, string? citizenName = null);
    Task<HouseholdMember?> GetMemberByIdAsync(string memberId, string? citizenName = null);
    Task<HouseholdMember> AddMemberAsync(HouseholdMember member, string? citizenName = null);
    Task<HouseholdMember?> UpdateMemberAsync(HouseholdMember member, string? citizenName = null);
    Task<bool> DeleteMemberAsync(string memberId, string? citizenName = null);
    Task<IEnumerable<ReadinessChecklistItem>> ToggleReadinessItemAsync(string key, bool? isCompleted = null, string? citizenName = null);
}
