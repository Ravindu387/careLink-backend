using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryPartnerOrganisationRepository : IPartnerOrganisationRepository
{
    private readonly ConcurrentDictionary<string, PartnerOrganisation> _organisations = new();
    private const int BaselineActivePartners = 38;
    private const int BaselineTotalPartners = 40;
    private const int BaselineSharedResources = 88;
    private const int BaselineCollaborationRequests = 3;

    public InMemoryPartnerOrganisationRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<PartnerOrganisation>> GetAllAsync(
        PartnerOrganisationFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _organisations.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(o =>
                    o.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    o.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    o.Type.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    o.Region.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    o.Role.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    o.Email.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    o.Phone.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    (o.ContactPerson != null && o.ContactPerson.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (o.Notes != null && o.Notes.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Region) &&
                !filter.Region.Equals("All operational areas", StringComparison.OrdinalIgnoreCase))
            {
                var r = filter.Region.Trim();
                query = query.Where(o => o.Region.Contains(r, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                var st = filter.Status.Trim();
                query = query.Where(o => o.Status.Equals(st, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Verification) &&
                !filter.Verification.Equals("Any availability", StringComparison.OrdinalIgnoreCase))
            {
                var v = filter.Verification.Trim();
                query = query.Where(o => o.Verification.Equals(v, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Type) &&
                !filter.Type.Equals("All types", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(o => o.Type.Equals(filter.Type.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Role) &&
                !filter.Role.Equals("All roles", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(o => o.Role.Equals(filter.Role.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderBy(o => o.Id)
            .ToList();

        return Task.FromResult<IReadOnlyList<PartnerOrganisation>>(list);
    }

    public Task<PartnerOrganisation?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _organisations.TryGetValue(id, out var org);
        return Task.FromResult(org);
    }

    public Task<PartnerOrganisation> AddAsync(PartnerOrganisation organisation, CancellationToken cancellationToken = default)
    {
        _organisations[organisation.Id] = organisation;
        return Task.FromResult(organisation);
    }

    public Task<PartnerOrganisation?> UpdateAsync(PartnerOrganisation organisation, CancellationToken cancellationToken = default)
    {
        if (!_organisations.ContainsKey(organisation.Id))
        {
            return Task.FromResult<PartnerOrganisation?>(null);
        }

        _organisations[organisation.Id] = organisation;
        return Task.FromResult<PartnerOrganisation?>(organisation);
    }

    public Task<PartnerOrganisation?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        if (!_organisations.TryGetValue(id, out var org))
        {
            return Task.FromResult<PartnerOrganisation?>(null);
        }

        org.Status = status;
        _organisations[id] = org;
        return Task.FromResult<PartnerOrganisation?>(org);
    }

    public Task<PartnerOrganisation?> UpdateVerificationAsync(string id, string verification, CancellationToken cancellationToken = default)
    {
        if (!_organisations.TryGetValue(id, out var org))
        {
            return Task.FromResult<PartnerOrganisation?>(null);
        }

        org.Verification = verification;
        if (verification.Equals("Verified", StringComparison.OrdinalIgnoreCase))
        {
            org.VerifiedAt = DateTimeOffset.UtcNow;
        }
        _organisations[id] = org;
        return Task.FromResult<PartnerOrganisation?>(org);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var removed = _organisations.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<PartnerOrganisationsMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var items = _organisations.Values.ToList();

        var activeLive = items.Count(o => o.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
        var totalLive = items.Count;
        var sharedResourcesLive = items.Sum(o => o.SharedResourcesCount);
        var collabRequestsLive = items.Sum(o => o.CollaborationRequestsCount);
        var pendingReviews = items.Count(o => o.Verification.Equals("Pending", StringComparison.OrdinalIgnoreCase));
        var verifiedCount = items.Count(o => o.Verification.Equals("Verified", StringComparison.OrdinalIgnoreCase));

        var activeTotal = BaselineActivePartners + activeLive;
        var totalAll = BaselineTotalPartners + totalLive;
        var verifiedTotal = BaselineActivePartners + verifiedCount;
        var coveragePercent = totalAll > 0 ? (int)Math.Round((double)verifiedTotal / totalAll * 100) : 96;

        var metrics = new PartnerOrganisationsMetrics
        {
            ActivePartnersCount = activeTotal,
            TotalPartnersCount = totalAll,
            SharedResourcesCount = BaselineSharedResources + sharedResourcesLive,
            CollaborationRequestsCount = BaselineCollaborationRequests + collabRequestsLive,
            PendingCollaborationRequestsCount = 5,
            VerificationCoveragePercent = coveragePercent,
            PendingVerificationReviewsCount = pendingReviews,
            SectorsCount = 6,
            LastUpdated = DateTimeOffset.UtcNow
        };

        return Task.FromResult(metrics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _organisations.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;

        var list = new List<PartnerOrganisation>
        {
            new()
            {
                Id = "ORG-001",
                Name = "National Disaster Authority",
                Type = "Government",
                Region = "Greater Riverton",
                Role = "Lead Coordinator",
                Verification = "Verified",
                Status = "Active",
                Email = "contact@nda.org",
                Phone = "+94 11 234 5678",
                ContactPerson = "Director General K. Wickremasinghe",
                SharedResourcesCount = 42,
                CollaborationRequestsCount = 4,
                Address = "Command Complex, Central Ward, Riverton",
                Website = "https://nda.gov.lk",
                Notes = "Primary government disaster management authority with highest escalation authority.",
                CreatedAt = now.AddDays(-60),
                VerifiedAt = now.AddDays(-55)
            },
            new()
            {
                Id = "ORG-014",
                Name = "CareLink Shelter Network",
                Type = "Local NGO",
                Region = "Central - North",
                Role = "Shelter Operations",
                Verification = "Verified",
                Status = "Active",
                Email = "info@carelink.org",
                Phone = "+94 11 234 5679",
                ContactPerson = "Sarah Perera",
                SharedResourcesCount = 36,
                CollaborationRequestsCount = 3,
                Address = "45 Lakeview Crescent, Riverton",
                Website = "https://shelternetwork.carelink.org",
                Notes = "Coordinates temporary shelters, evacuation centres, and bedding distribution.",
                CreatedAt = now.AddDays(-45),
                VerifiedAt = now.AddDays(-40)
            },
            new()
            {
                Id = "ORG-027",
                Name = "Global Relief Network",
                Type = "International NGO",
                Region = "4 districts",
                Role = "Food - Medical",
                Verification = "Verified",
                Status = "Active",
                Email = "support@globalrelief.org",
                Phone = "+94 11 234 5680",
                ContactPerson = "Marcus Vance",
                SharedResourcesCount = 32,
                CollaborationRequestsCount = 2,
                Address = "UN Humanitarian Hub, Riverton Port",
                Website = "https://globalrelief.org",
                Notes = "International logistics, emergency medical response, and field hospital kits.",
                CreatedAt = now.AddDays(-30),
                VerifiedAt = now.AddDays(-28)
            },
            new()
            {
                Id = "ORG-042",
                Name = "Emergency Logistics Unit",
                Type = "Government",
                Region = "Central Region",
                Role = "Transport - Warehousing",
                Verification = "Verified",
                Status = "Active",
                Email = "logistics@example.org",
                Phone = "+94 11 234 5681",
                ContactPerson = "Major T. Silva",
                SharedResourcesCount = 20,
                CollaborationRequestsCount = 2,
                Address = "Warehouse Yard 4, Eastbank Industrial Zone",
                Website = "https://elu.gov.lk",
                Notes = "High-clearance transport trucks, mobile water bowsers, and regional depots.",
                CreatedAt = now.AddDays(-25),
                VerifiedAt = now.AddDays(-22)
            },
            new()
            {
                Id = "ORG-005",
                Name = "Red Crescent Society - Riverton Chapter",
                Type = "International NGO",
                Region = "Greater Riverton",
                Role = "Food - Medical",
                Verification = "Verified",
                Status = "Active",
                Email = "riverton@redcrescent.org",
                Phone = "+94 11 234 5682",
                ContactPerson = "Dr. Amina Farooq",
                SharedResourcesCount = 18,
                CollaborationRequestsCount = 1,
                Address = "12 Crescent Way, Riverton",
                Website = "https://redcrescent.org",
                Notes = "Ambulance units, first aid supply chain, and emergency medical technicians.",
                CreatedAt = now.AddDays(-20),
                VerifiedAt = now.AddDays(-18)
            },
            new()
            {
                Id = "ORG-008",
                Name = "Civic Rescue Foundation",
                Type = "Local NGO",
                Region = "Eastbank",
                Role = "Emergency Assistance",
                Verification = "Verified",
                Status = "Active",
                Email = "team@civicrescue.org",
                Phone = "+94 11 234 5683",
                ContactPerson = "Rohan Jayawardena",
                SharedResourcesCount = 14,
                CollaborationRequestsCount = 1,
                Address = "88 Riverside Avenue, Eastbank",
                Website = "https://civicrescue.lk",
                Notes = "Flood boat rescue operations, swiftwater extraction, and drone scouting.",
                CreatedAt = now.AddDays(-15),
                VerifiedAt = now.AddDays(-12)
            },
            new()
            {
                Id = "ORG-012",
                Name = "Metro Community Kitchens",
                Type = "Community Organisation",
                Region = "Central Ward",
                Role = "Food - Medical",
                Verification = "Pending",
                Status = "Active",
                Email = "kitchens@metrocare.org",
                Phone = "+94 11 234 5684",
                ContactPerson = "Priya Gunasekera",
                SharedResourcesCount = 8,
                CollaborationRequestsCount = 1,
                Address = "Old Town Community Center, Central Ward",
                Notes = "Hot meal preparation up to 4,000 rations daily. Food safety audit pending.",
                CreatedAt = now.AddDays(-5),
                VerifiedAt = null
            },
            new()
            {
                Id = "ORG-018",
                Name = "Apex Telecom Disaster Wing",
                Type = "Private Organisation",
                Region = "Greater Riverton",
                Role = "Transport - Warehousing",
                Verification = "Verified",
                Status = "Active",
                Email = "disaster-ops@apextelecom.org",
                Phone = "+94 11 234 5685",
                ContactPerson = "Dinesh Bandara",
                SharedResourcesCount = 12,
                CollaborationRequestsCount = 0,
                Address = "Apex Tech Tower, Level 8, Riverton",
                Website = "https://apextelecom.lk/emergency",
                Notes = "Mobile satellite uplink vans, emergency Wi-Fi hubs, generator trailers.",
                CreatedAt = now.AddDays(-10),
                VerifiedAt = now.AddDays(-8)
            },
            new()
            {
                Id = "ORG-023",
                Name = "North District Shelter Collective",
                Type = "Local NGO",
                Region = "North Ward",
                Role = "Shelter Operations",
                Verification = "Verified",
                Status = "Active",
                Email = "shelters@northcollective.org",
                Phone = "+94 11 234 5686",
                ContactPerson = "Kamala Weerasinghe",
                SharedResourcesCount = 11,
                CollaborationRequestsCount = 1,
                Address = "Community Hall Complex, North Ward",
                Notes = "Coordinates 5 temporary camps in schools and temples across North Ward.",
                CreatedAt = now.AddDays(-14),
                VerifiedAt = now.AddDays(-11)
            },
            new()
            {
                Id = "ORG-031",
                Name = "Harbor Health Volunteer Corps",
                Type = "Community Organisation",
                Region = "Eastbank",
                Role = "Emergency Assistance",
                Verification = "Pending",
                Status = "Active",
                Email = "volunteer@harborhealth.org",
                Phone = "+94 11 234 5687",
                ContactPerson = "Nalinda Fernando",
                SharedResourcesCount = 5,
                CollaborationRequestsCount = 0,
                Address = "Harbor Fishermen Welfare Center, Eastbank",
                Notes = "Local paramedic volunteers and triage dispatchers. Credential verification under review.",
                CreatedAt = now.AddDays(-2),
                VerifiedAt = null
            }
        };

        foreach (var org in list)
        {
            _organisations[org.Id] = org;
        }
    }
}
