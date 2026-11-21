using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryOrganisationProfileRepository : IOrganisationProfileRepository
{
    private readonly ConcurrentDictionary<string, OrganisationProfile> _profiles = new();
    private readonly object _lock = new();

    public InMemoryOrganisationProfileRepository()
    {
        SeedDefaultProfile();
    }

    public Task<OrganisationProfile> GetProfileAsync(
        string orgId = "ORG-027",
        CancellationToken cancellationToken = default)
    {
        var key = string.IsNullOrWhiteSpace(orgId) ? "ORG-027" : orgId.Trim();
        if (_profiles.TryGetValue(key, out var profile))
        {
            return Task.FromResult(Clone(profile));
        }

        var defaultProfile = CreateDefaultProfile();
        _profiles[key] = defaultProfile;
        return Task.FromResult(Clone(defaultProfile));
    }

    public Task<OrganisationProfile> UpdateProfileAsync(
        OrganisationProfile profile,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _profiles[profile.Id] = Clone(profile);
            return Task.FromResult(Clone(profile));
        }
    }

    public Task<OrganisationProfile> UpdateCapabilitiesAsync(
        string orgId,
        OrganisationCapabilities capabilities,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var key = string.IsNullOrWhiteSpace(orgId) ? "ORG-027" : orgId.Trim();
            if (!_profiles.TryGetValue(key, out var profile))
            {
                profile = CreateDefaultProfile();
                profile.Id = key;
            }

            profile.Capabilities = capabilities;
            profile.UpdatedAt = DateTimeOffset.UtcNow;
            _profiles[key] = profile;

            return Task.FromResult(Clone(profile));
        }
    }

    public Task<OrganisationProfile> ResetDefaultProfileAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _profiles.Clear();
            var seeded = SeedDefaultProfile();
            return Task.FromResult(Clone(seeded));
        }
    }

    private OrganisationProfile SeedDefaultProfile()
    {
        var profile = CreateDefaultProfile();
        _profiles[profile.Id] = profile;
        return profile;
    }

    private static OrganisationProfile CreateDefaultProfile()
    {
        return new OrganisationProfile
        {
            Id = "ORG-027",
            Name = "Global Relief Network",
            LegalName = "Global Relief Network International",
            RegistrationNumber = "NGO-2011-0482",
            Type = "International NGO",
            RegisteredDate = "18 March 2011",
            Status = "Active",
            Verification = "Verified",
            HeadOffice = "Central Ward, Greater Riverton",
            PrimaryContact = "Sara Khan - Partner Officer",
            ContactEmail = "sara@globalrelief.org",
            EmergencyLine = "+94 11 555 0142",
            Website = "https://globalrelief.org",
            ProfileCompleteness = 94,
            CompletenessReviewItems = 2,
            Sectors = new List<string>
            {
                "Food Security",
                "WASH",
                "Community Health",
                "Emergency Logistics"
            },
            Verifications = new List<OrganisationVerificationItem>
            {
                new()
                {
                    Title = "Organisation verification",
                    Subtitle = "Verified",
                    Status = "Current",
                    Done = true
                },
                new()
                {
                    Title = "Data sharing agreement",
                    Subtitle = "Active · expires 18 Mar 2026",
                    Status = "Current",
                    Done = true
                },
                new()
                {
                    Title = "Safeguarding attestation",
                    Subtitle = "Review due in 18 days",
                    Status = "Due Soon",
                    Done = false
                },
                new()
                {
                    Title = "Banking verification",
                    Subtitle = "Verified",
                    Status = "Current",
                    Done = true
                }
            },
            Capabilities = new OrganisationCapabilities
            {
                Personnel = new CapabilityMetric
                {
                    Title = "Personnel",
                    Value = "84 active staff",
                    Subtitle = "62 deployable today",
                    TotalCount = 84,
                    ActiveCount = 62
                },
                Fleet = new CapabilityMetric
                {
                    Title = "Fleet",
                    Value = "18 vehicles",
                    Subtitle = "14 available",
                    TotalCount = 18,
                    ActiveCount = 14
                },
                Stock = new CapabilityMetric
                {
                    Title = "Stock",
                    Value = "8 resource categories",
                    Subtitle = "68% available",
                    TotalCount = 8,
                    ActiveCount = 68
                },
                Coverage = new CapabilityMetric
                {
                    Title = "Coverage",
                    Value = "6 districts",
                    Subtitle = "4 active now",
                    TotalCount = 6,
                    ActiveCount = 4
                }
            },
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static OrganisationProfile Clone(OrganisationProfile p)
    {
        return new OrganisationProfile
        {
            Id = p.Id,
            Name = p.Name,
            LegalName = p.LegalName,
            RegistrationNumber = p.RegistrationNumber,
            Type = p.Type,
            RegisteredDate = p.RegisteredDate,
            Status = p.Status,
            Verification = p.Verification,
            HeadOffice = p.HeadOffice,
            PrimaryContact = p.PrimaryContact,
            ContactEmail = p.ContactEmail,
            EmergencyLine = p.EmergencyLine,
            Website = p.Website,
            ProfileCompleteness = p.ProfileCompleteness,
            CompletenessReviewItems = p.CompletenessReviewItems,
            Sectors = new List<string>(p.Sectors),
            Verifications = p.Verifications.Select(v => new OrganisationVerificationItem
            {
                Title = v.Title,
                Subtitle = v.Subtitle,
                Status = v.Status,
                Done = v.Done,
                DueDate = v.DueDate
            }).ToList(),
            Capabilities = new OrganisationCapabilities
            {
                Personnel = new CapabilityMetric
                {
                    Title = p.Capabilities.Personnel.Title,
                    Value = p.Capabilities.Personnel.Value,
                    Subtitle = p.Capabilities.Personnel.Subtitle,
                    TotalCount = p.Capabilities.Personnel.TotalCount,
                    ActiveCount = p.Capabilities.Personnel.ActiveCount
                },
                Fleet = new CapabilityMetric
                {
                    Title = p.Capabilities.Fleet.Title,
                    Value = p.Capabilities.Fleet.Value,
                    Subtitle = p.Capabilities.Fleet.Subtitle,
                    TotalCount = p.Capabilities.Fleet.TotalCount,
                    ActiveCount = p.Capabilities.Fleet.ActiveCount
                },
                Stock = new CapabilityMetric
                {
                    Title = p.Capabilities.Stock.Title,
                    Value = p.Capabilities.Stock.Value,
                    Subtitle = p.Capabilities.Stock.Subtitle,
                    TotalCount = p.Capabilities.Stock.TotalCount,
                    ActiveCount = p.Capabilities.Stock.ActiveCount
                },
                Coverage = new CapabilityMetric
                {
                    Title = p.Capabilities.Coverage.Title,
                    Value = p.Capabilities.Coverage.Value,
                    Subtitle = p.Capabilities.Coverage.Subtitle,
                    TotalCount = p.Capabilities.Coverage.TotalCount,
                    ActiveCount = p.Capabilities.Coverage.ActiveCount
                }
            },
            UpdatedAt = p.UpdatedAt
        };
    }
}
