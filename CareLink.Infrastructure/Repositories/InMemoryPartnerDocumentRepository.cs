using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryPartnerDocumentRepository : IPartnerDocumentRepository
{
    private readonly ConcurrentDictionary<string, PartnerDocument> _documents = new();

    public InMemoryPartnerDocumentRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<PartnerDocument>> GetAllAsync(
        DocumentFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _documents.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(d =>
                    d.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    d.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    d.Type.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    d.Owner.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    d.Visibility.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    (d.FileName != null && d.FileName.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Tab) &&
                !filter.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var tab = filter.Tab.Trim();
                if (tab.Equals("Needs action", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(d =>
                        d.Status.Equals("Awaiting Signature", StringComparison.OrdinalIgnoreCase) ||
                        d.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(d =>
                        d.Status.Equals("Published", StringComparison.OrdinalIgnoreCase) ||
                        d.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(d => d.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Archived", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(d => d.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Type) &&
                !filter.Type.Equals("All document types", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => d.Type.Equals(filter.Type.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Owner) &&
                !filter.Owner.Equals("All owners", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => d.Owner.Equals(filter.Owner.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Visibility) &&
                !filter.Visibility.Equals("All visibility", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => d.Visibility.Equals(filter.Visibility.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => d.Status.Equals(filter.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderByDescending(d => d.UpdatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<PartnerDocument>>(list);
    }

    public Task<PartnerDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _documents.TryGetValue(id, out var doc);
        return Task.FromResult(doc);
    }

    public Task<PartnerDocument> AddAsync(PartnerDocument document, CancellationToken cancellationToken = default)
    {
        _documents[document.Id] = document;
        return Task.FromResult(document);
    }

    public Task<PartnerDocument?> UpdateAsync(PartnerDocument document, CancellationToken cancellationToken = default)
    {
        if (!_documents.ContainsKey(document.Id))
        {
            return Task.FromResult<PartnerDocument?>(null);
        }

        document.UpdatedAt = DateTimeOffset.UtcNow;
        _documents[document.Id] = document;
        return Task.FromResult<PartnerDocument?>(document);
    }

    public Task<PartnerDocument?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        if (!_documents.TryGetValue(id, out var doc))
        {
            return Task.FromResult<PartnerDocument?>(null);
        }

        doc.Status = status.Trim();
        doc.UpdatedAt = DateTimeOffset.UtcNow;
        doc.Updated = doc.UpdatedAt.ToString("dd MMM - HH:mm");
        _documents[id] = doc;
        return Task.FromResult<PartnerDocument?>(doc);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_documents.TryRemove(id, out _));
    }

    public Task<PartnerDocumentMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = _documents.Values.ToList();

        var activeRecords = all.Count(d =>
            !d.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));

        var needsReview = all.Count(d =>
            d.Status.Equals("Awaiting Signature", StringComparison.OrdinalIgnoreCase) ||
            d.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase));

        var activeCount = all.Count(d =>
            d.Status.Equals("Published", StringComparison.OrdinalIgnoreCase) ||
            d.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));

        var completedCount = all.Count(d =>
            d.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));

        var archivedCount = all.Count(d =>
            d.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));

        var metrics = new PartnerDocumentMetrics
        {
            ActiveRecords = Math.Max(activeRecords, 48),
            NeedsReview = needsReview,
            VerifiedPercentage = 96,
            DueSoon = 2,
            TotalDocuments = all.Count,
            NeedsActionCount = needsReview,
            ActiveCount = activeCount,
            CompletedCount = completedCount,
            ArchivedCount = archivedCount,
            LastUpdated = DateTimeOffset.UtcNow
        };

        return Task.FromResult(metrics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _documents.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;

        var seeds = new List<PartnerDocument>
        {
            new()
            {
                Id = "DOC-0412",
                Name = "Partner response plan - Floods 2025",
                Type = "Operational Plan",
                Owner = "Sara Khan",
                Updated = "22 Jun - 12:18",
                Version = "v3.2",
                Visibility = "Shared with NDA",
                Status = "Published",
                FileName = "partner_response_plan_floods_v3.2.pdf",
                FileUrl = "/sample-docs/response_plan.pdf",
                FileSizeBytes = 2450000,
                Description = "Primary operational strategy and inter-agency resource commitments for flood relief.",
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddHours(-1)
            },
            new()
            {
                Id = "DOC-0408",
                Name = "North Ward distribution schedule",
                Type = "Schedule",
                Owner = "Michael Torres",
                Updated = "22 Jun - 10:42",
                Version = "v2.1",
                Visibility = "8 organisations",
                Status = "Published",
                FileName = "north_ward_distribution_schedule_v2.xlsx",
                FileUrl = "/sample-docs/distribution_schedule.xlsx",
                FileSizeBytes = 680000,
                Description = "Detailed timetable for water bowsers and food ration distribution sites.",
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddHours(-3)
            },
            new()
            {
                Id = "DOC-0397",
                Name = "WASH Technical assessment",
                Type = "Assessment",
                Owner = "Nadia Silva",
                Updated = "21 Jun - 17:28",
                Version = "v1.0",
                Visibility = "Internal",
                Status = "Approved",
                FileName = "wash_tech_assessment_kelaniya.pdf",
                FileUrl = "/sample-docs/wash_assessment.pdf",
                FileSizeBytes = 1890000,
                Description = "Field assessment of municipal well contamination and emergency purification requirements.",
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = "DOC-0388",
                Name = "Data sharing agreement",
                Type = "Agreement",
                Owner = "Sara Khan",
                Updated = "19 Jun - 14:10",
                Version = "v1.4",
                Visibility = "NDA · CareLink",
                Status = "Awaiting Signature",
                FileName = "bilateral_data_sharing_nda_carelink.pdf",
                FileUrl = "/sample-docs/data_agreement.pdf",
                FileSizeBytes = 450000,
                Description = "Formal legal agreement governing citizen privacy and beneficiary registry synchronization.",
                CreatedAt = now.AddDays(-4),
                UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = "DOC-0382",
                Name = "Emergency shelter assessment",
                Type = "Assessment",
                Owner = "Michael Torres",
                Updated = "18 Jun - 11:30",
                Version = "v1.2",
                Visibility = "Internal",
                Status = "Under Review",
                FileName = "emergency_shelter_capacity_report.pdf",
                FileUrl = "/sample-docs/shelter_report.pdf",
                FileSizeBytes = 1200000,
                Description = "Structural safety review and maximum occupancy calculations for designated schools.",
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-3)
            },
            new()
            {
                Id = "DOC-0375",
                Name = "Food distribution agreement",
                Type = "Agreement",
                Owner = "Nadia Silva",
                Updated = "17 Jun - 09:15",
                Version = "v2.0",
                Visibility = "Shared with NDA",
                Status = "Archived",
                FileName = "mou_food_distribution_wfp_nda.pdf",
                FileUrl = "/sample-docs/food_mou.pdf",
                FileSizeBytes = 890000,
                Description = "Archived memorandum of understanding for initial flash flood emergency intake.",
                CreatedAt = now.AddDays(-7),
                UpdatedAt = now.AddDays(-4)
            }
        };

        foreach (var d in seeds)
        {
            _documents[d.Id] = d;
        }
    }
}
