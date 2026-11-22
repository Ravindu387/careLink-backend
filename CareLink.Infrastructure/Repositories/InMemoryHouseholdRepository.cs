namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryHouseholdRepository : IHouseholdRepository
{
    private readonly ConcurrentDictionary<string, HouseholdProfile> _households = new();
    private readonly Lock _lock = new();

    public InMemoryHouseholdRepository()
    {
        SeedInitialData();
    }

    public Task<HouseholdProfile> GetProfileAsync(string? citizenName = null)
    {
        var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();
        var profile = _households.Values.FirstOrDefault(h =>
            h.CitizenName.Equals(targetCitizen, StringComparison.OrdinalIgnoreCase));

        if (profile == null)
        {
            // Create a default profile if none exists for this citizen
            profile = new HouseholdProfile
            {
                Id = $"HH-{_households.Count + 1}",
                CitizenName = targetCitizen,
                CitizenId = "CIT-001",
                Address = "54 Riverside Lane, North Ward, Greater Riverton",
                Ward = "North Ward",
                City = "Greater Riverton",
                EmergencyContactName = "Rashid Khan",
                EmergencyContactPhone = "+94 77 455 0182",
                EmergencyContactRelationship = "Brother",
                Members = new List<HouseholdMember>
                {
                    new()
                    {
                        Id = "1",
                        Name = targetCitizen,
                        Initials = GetInitials(targetCitizen),
                        Role = "You · Primary contact",
                        SupportNeeds = "No support needs",
                        CreatedAt = DateTime.UtcNow
                    }
                },
                ReadinessChecklist = GetDefaultReadinessChecklist()
            };
            _households[profile.Id] = profile;
        }

        return Task.FromResult(profile);
    }

    public Task<HouseholdProfile> UpdateAddressAsync(string address, string? ward = null, string? city = null, string? citizenName = null)
    {
        lock (_lock)
        {
            var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();
            var profile = _households.Values.FirstOrDefault(h =>
                h.CitizenName.Equals(targetCitizen, StringComparison.OrdinalIgnoreCase)) ?? GetProfileAsync(targetCitizen).Result;

            profile.Address = address.Trim();
            if (!string.IsNullOrWhiteSpace(ward)) profile.Ward = ward.Trim();
            if (!string.IsNullOrWhiteSpace(city)) profile.City = city.Trim();
            profile.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult(profile);
        }
    }

    public Task<HouseholdProfile> UpdateEmergencyContactAsync(string contactName, string contactPhone, string? relationship = null, string? citizenName = null)
    {
        lock (_lock)
        {
            var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();
            var profile = _households.Values.FirstOrDefault(h =>
                h.CitizenName.Equals(targetCitizen, StringComparison.OrdinalIgnoreCase)) ?? GetProfileAsync(targetCitizen).Result;

            profile.EmergencyContactName = contactName.Trim();
            profile.EmergencyContactPhone = contactPhone.Trim();
            if (!string.IsNullOrWhiteSpace(relationship)) profile.EmergencyContactRelationship = relationship.Trim();
            profile.UpdatedAt = DateTime.UtcNow;

            // Mark emergency contact readiness item as complete
            var contactItem = profile.ReadinessChecklist.FirstOrDefault(r => r.Key == "emergency_contact");
            if (contactItem != null)
            {
                contactItem.IsCompleted = true;
            }

            return Task.FromResult(profile);
        }
    }

    public Task<HouseholdMember?> GetMemberByIdAsync(string memberId, string? citizenName = null)
    {
        var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();
        var profile = _households.Values.FirstOrDefault(h =>
            h.CitizenName.Equals(targetCitizen, StringComparison.OrdinalIgnoreCase)) ?? GetProfileAsync(targetCitizen).Result;

        var member = profile.Members.FirstOrDefault(m => m.Id == memberId);
        return Task.FromResult(member);
    }

    public Task<HouseholdMember> AddMemberAsync(HouseholdMember member, string? citizenName = null)
    {
        lock (_lock)
        {
            var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();
            var profile = _households.Values.FirstOrDefault(h =>
                h.CitizenName.Equals(targetCitizen, StringComparison.OrdinalIgnoreCase)) ?? GetProfileAsync(targetCitizen).Result;

            if (string.IsNullOrWhiteSpace(member.Id))
            {
                member.Id = (profile.Members.Count > 0 ? profile.Members.Max(m => int.TryParse(m.Id, out var i) ? i : 0) + 1 : 1).ToString();
            }

            if (string.IsNullOrWhiteSpace(member.Initials))
            {
                member.Initials = GetInitials(member.Name);
            }

            if (member.CreatedAt == default)
            {
                member.CreatedAt = DateTime.UtcNow;
            }

            profile.Members.Add(member);
            profile.UpdatedAt = DateTime.UtcNow;

            // Mark members added readiness item as complete
            var membersItem = profile.ReadinessChecklist.FirstOrDefault(r => r.Key == "members_added");
            if (membersItem != null)
            {
                membersItem.IsCompleted = true;
            }

            return Task.FromResult(member);
        }
    }

    public Task<HouseholdMember?> UpdateMemberAsync(HouseholdMember member, string? citizenName = null)
    {
        lock (_lock)
        {
            var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();
            var profile = _households.Values.FirstOrDefault(h =>
                h.CitizenName.Equals(targetCitizen, StringComparison.OrdinalIgnoreCase)) ?? GetProfileAsync(targetCitizen).Result;

            var existing = profile.Members.FirstOrDefault(m => m.Id == member.Id);
            if (existing == null)
            {
                return Task.FromResult<HouseholdMember?>(null);
            }

            existing.Name = member.Name;
            existing.Initials = string.IsNullOrWhiteSpace(member.Initials) ? GetInitials(member.Name) : member.Initials;
            existing.Role = member.Role;
            existing.SupportNeeds = member.SupportNeeds;
            existing.Age = member.Age;
            existing.MedicalNotes = member.MedicalNotes;
            existing.DietaryNotes = member.DietaryNotes;
            existing.UpdatedAt = DateTime.UtcNow;

            profile.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<HouseholdMember?>(existing);
        }
    }

    public Task<bool> DeleteMemberAsync(string memberId, string? citizenName = null)
    {
        lock (_lock)
        {
            var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();
            var profile = _households.Values.FirstOrDefault(h =>
                h.CitizenName.Equals(targetCitizen, StringComparison.OrdinalIgnoreCase)) ?? GetProfileAsync(targetCitizen).Result;

            var removed = profile.Members.RemoveAll(m => m.Id == memberId) > 0;
            if (removed)
            {
                profile.UpdatedAt = DateTime.UtcNow;
            }

            return Task.FromResult(removed);
        }
    }

    public Task<IEnumerable<ReadinessChecklistItem>> ToggleReadinessItemAsync(string key, bool? isCompleted = null, string? citizenName = null)
    {
        lock (_lock)
        {
            var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();
            var profile = _households.Values.FirstOrDefault(h =>
                h.CitizenName.Equals(targetCitizen, StringComparison.OrdinalIgnoreCase)) ?? GetProfileAsync(targetCitizen).Result;

            var item = profile.ReadinessChecklist.FirstOrDefault(r => r.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.IsCompleted = isCompleted ?? !item.IsCompleted;
                profile.UpdatedAt = DateTime.UtcNow;
            }

            return Task.FromResult<IEnumerable<ReadinessChecklistItem>>(profile.ReadinessChecklist);
        }
    }

    private void SeedInitialData()
    {
        var samiraHousehold = new HouseholdProfile
        {
            Id = "HH-001",
            CitizenName = "Samira Khan",
            CitizenId = "CIT-001",
            Address = "54 Riverside Lane, North Ward, Greater Riverton",
            Ward = "North Ward",
            City = "Greater Riverton",
            EmergencyContactName = "Rashid Khan",
            EmergencyContactPhone = "+94 77 455 0182",
            EmergencyContactRelationship = "Brother",
            Members = new List<HouseholdMember>
            {
                new()
                {
                    Id = "1",
                    Name = "Samira Khan",
                    Initials = "SK",
                    Role = "You · Primary contact",
                    SupportNeeds = "No support needs",
                    Age = 34,
                    CreatedAt = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = "2",
                    Name = "Amir Khan",
                    Initials = "AK",
                    Role = "Household member",
                    SupportNeeds = "Limited mobility",
                    Age = 68,
                    MedicalNotes = "Requires wheelchair access and ground-floor shelter placement.",
                    CreatedAt = new DateTime(2025, 6, 15, 10, 5, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = "3",
                    Name = "Layla Khan",
                    Initials = "LK",
                    Role = "Household member",
                    SupportNeeds = "Asthma inhaler",
                    Age = 12,
                    MedicalNotes = "Requires salbutamol inhaler; dust-free environment requested.",
                    CreatedAt = new DateTime(2025, 6, 15, 10, 10, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = "4",
                    Name = "Mariam Khan",
                    Initials = "MK",
                    Role = "Household member",
                    SupportNeeds = "Daily medication",
                    Age = 64,
                    MedicalNotes = "Hypertension and diabetes medication maintenance.",
                    CreatedAt = new DateTime(2025, 6, 15, 10, 15, 0, DateTimeKind.Utc)
                }
            },
            ReadinessChecklist = GetDefaultReadinessChecklist()
        };

        _households[samiraHousehold.Id] = samiraHousehold;
    }

    private static List<ReadinessChecklistItem> GetDefaultReadinessChecklist()
    {
        return new List<ReadinessChecklistItem>
        {
            new() { Key = "members_added", Title = "Household members added", IsCompleted = true, ActionText = "Review" },
            new() { Key = "support_reviewed", Title = "Support needs reviewed", IsCompleted = true, ActionText = "Review" },
            new() { Key = "emergency_contact", Title = "Emergency contact added", IsCompleted = true, ActionText = "Review" },
            new() { Key = "documents_uploaded", Title = "Important documents uploaded", IsCompleted = false, ActionText = "Add now" }
        };
    }

    private static string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "HM";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][0].ToString().ToUpperInvariant();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }
}
