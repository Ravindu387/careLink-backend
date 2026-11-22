namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly ConcurrentDictionary<string, CitizenDocument> _documents = new();
    private readonly Lock _lock = new();

    public InMemoryDocumentRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<CitizenDocument>> GetAllAsync(
        string? citizenName = null,
        string? category = null,
        string? search = null)
    {
        var query = _documents.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            query = query.Where(d => d.CitizenName.Equals(citizenName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(d => d.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(d =>
                d.Name.ToLowerInvariant().Contains(s) ||
                d.Description.ToLowerInvariant().Contains(s) ||
                d.Category.ToLowerInvariant().Contains(s) ||
                d.FileName.ToLowerInvariant().Contains(s) ||
                d.Status.ToLowerInvariant().Contains(s)
            );
        }

        var sorted = query.OrderByDescending(d => d.CreatedAt).ToList();
        return Task.FromResult<IEnumerable<CitizenDocument>>(sorted);
    }

    public Task<CitizenDocument?> GetByIdAsync(string id)
    {
        _documents.TryGetValue(id, out var document);
        return Task.FromResult(document);
    }

    public Task<CitizenDocument> CreateAsync(CitizenDocument document)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(document.Id))
            {
                document.Id = $"DOC-{_documents.Count + 1}";
            }

            if (document.CreatedAt == default)
            {
                document.CreatedAt = DateTime.UtcNow;
            }

            _documents[document.Id] = document;
            return Task.FromResult(document);
        }
    }

    public Task<CitizenDocument?> UpdateAsync(CitizenDocument document)
    {
        lock (_lock)
        {
            if (!_documents.ContainsKey(document.Id))
            {
                return Task.FromResult<CitizenDocument?>(null);
            }

            document.UpdatedAt = DateTime.UtcNow;
            _documents[document.Id] = document;
            return Task.FromResult<CitizenDocument?>(document);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_documents.TryRemove(id, out _));
    }

    public Task<DocumentSummaryDto> GetSummaryAsync(string? citizenName = null)
    {
        var query = _documents.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            query = query.Where(d => d.CitizenName.Equals(citizenName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        var list = query.ToList();
        var summary = new DocumentSummaryDto
        {
            TotalDocuments = list.Count,
            VerifiedCount = list.Count(d => d.Status.Equals("Verified", StringComparison.OrdinalIgnoreCase)),
            UnderReviewCount = list.Count(d => d.Status.Equals("Under review", StringComparison.OrdinalIgnoreCase)),
            SavedCount = list.Count(d => d.Status.Equals("Saved", StringComparison.OrdinalIgnoreCase))
        };

        return Task.FromResult(summary);
    }

    private void SeedInitialData()
    {
        var doc1 = new CitizenDocument
        {
            Id = "1",
            Name = "Identity document",
            Category = "Identity",
            Description = "National ID · Samira Khan",
            FileName = "national_identity_card_samira.pdf",
            FileType = "application/pdf",
            FileSizeBytes = 1258291, // 1.2 MB
            FileSizeFormatted = "1.2 MB",
            Status = "Verified",
            VerificationNotes = "Verified by Civil Registration Officer Elena Vance on 16 Jun 2025.",
            CitizenName = "Samira Khan",
            CitizenId = "CIT-001",
            CreatedAt = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        var doc2 = new CitizenDocument
        {
            Id = "2",
            Name = "Medical information",
            Category = "Medical",
            Description = "Medication list · Mariam Khan",
            FileName = "prescription_and_mobility_records.pdf",
            FileType = "application/pdf",
            FileSizeBytes = 838860, // 820 KB
            FileSizeFormatted = "820 KB",
            Status = "Saved",
            VerificationNotes = "Uploaded for emergency shelter special mobility and prescription accommodation.",
            CitizenName = "Samira Khan",
            CitizenId = "CIT-001",
            CreatedAt = new DateTime(2025, 6, 18, 14, 15, 0, DateTimeKind.Utc)
        };

        var doc3 = new CitizenDocument
        {
            Id = "3",
            Name = "Proof of address",
            Category = "Address",
            Description = "Utility statement · May 2025",
            FileName = "water_board_utility_statement_may2025.pdf",
            FileType = "application/pdf",
            FileSizeBytes = 2097152, // 2.0 MB
            FileSizeFormatted = "2.0 MB",
            Status = "Verified",
            VerificationNotes = "Address confirmed: North Ward Sector 4, Riverton.",
            CitizenName = "Samira Khan",
            CitizenId = "CIT-001",
            CreatedAt = new DateTime(2025, 6, 20, 9, 45, 0, DateTimeKind.Utc)
        };

        var doc4 = new CitizenDocument
        {
            Id = "4",
            Name = "Emergency Evacuation Pass",
            Category = "Other",
            Description = "Municipal Council Priority Transit Authorization",
            FileName = "riverton_evacuation_pass_2025.pdf",
            FileType = "application/pdf",
            FileSizeBytes = 524288, // 512 KB
            FileSizeFormatted = "512 KB",
            Status = "Verified",
            VerificationNotes = "Official transit clearance for flood evacuation corridor B.",
            CitizenName = "Samira Khan",
            CitizenId = "CIT-001",
            CreatedAt = new DateTime(2025, 6, 21, 16, 20, 0, DateTimeKind.Utc)
        };

        _documents[doc1.Id] = doc1;
        _documents[doc2.Id] = doc2;
        _documents[doc3.Id] = doc3;
        _documents[doc4.Id] = doc4;
    }
}
