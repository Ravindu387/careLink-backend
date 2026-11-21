using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class DuplicatesService : IDuplicatesService
{
    private readonly List<DuplicateCaseItem> _duplicates = [];
    private readonly object _lock = new();

    public DuplicatesService()
    {
        InitializeSeedData();
    }

    private void InitializeSeedData()
    {
        var items = new List<DuplicateCaseItem>
        {
            new()
            {
                Id = "DUP-118",
                PrimaryCase = "REQ-2841",
                DuplicateCase = "REQ-2798",
                Factors = "Same phone · 120m proximity · Similar household",
                Score = 92,
                Confidence = "High",
                Status = "Awaiting Review",
                Category = "Emergency shelter",
                Detected = "10 min ago",
                Description = "Both requests contain similar applicant contact information and household details.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new()
            {
                Id = "DUP-114",
                PrimaryCase = "REQ-2836",
                DuplicateCase = "REQ-2811",
                Factors = "Same address · Matching category",
                Score = 88,
                Confidence = "High",
                Status = "Awaiting Review",
                Category = "Food & water",
                Detected = "25 min ago",
                Description = "Both requests have the same reported address and assistance category.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-25)
            },
            new()
            {
                Id = "DUP-107",
                PrimaryCase = "REQ-2789",
                DuplicateCase = "REQ-2774",
                Factors = "Shared contact · Similar description",
                Score = 76,
                Confidence = "Medium",
                Status = "Deferred",
                Category = "Medical supplies",
                Detected = "1 hour ago",
                Description = "Requests share contact information and have similar descriptions.",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = "DUP-102",
                PrimaryCase = "REQ-2761",
                DuplicateCase = "REQ-2742",
                Factors = "80m proximity · Same family size",
                Score = 69,
                Confidence = "Low",
                Status = "Awaiting Review",
                Category = "Emergency shelter",
                Detected = "2 hours ago",
                Description = "Both requests were submitted from nearby locations with matching household sizes.",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = "DUP-099",
                PrimaryCase = "REQ-2735",
                DuplicateCase = "REQ-2721",
                Factors = "Same phone · Matching location",
                Score = 95,
                Confidence = "High",
                Status = "Awaiting Review",
                Category = "Rescue assistance",
                Detected = "3 hours ago",
                Description = "The applicant phone number and reported location match.",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                Id = "DUP-095",
                PrimaryCase = "REQ-2701",
                DuplicateCase = "REQ-2689",
                Factors = "Similar description · Same category",
                Score = 81,
                Confidence = "Medium",
                Status = "Deferred",
                Category = "Food & water",
                Detected = "4 hours ago",
                Description = "The descriptions and assistance categories are similar.",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new()
            {
                Id = "DUP-091",
                PrimaryCase = "REQ-2650",
                DuplicateCase = "REQ-2632",
                Factors = "Same address · Shared contact",
                Score = 97,
                Confidence = "High",
                Status = "Confirmed",
                Category = "Medical supplies",
                Detected = "5 hours ago",
                Description = "The requests have matching contact details and address information.",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new()
            {
                Id = "DUP-087",
                PrimaryCase = "REQ-2605",
                DuplicateCase = "REQ-2588",
                Factors = "Nearby location · Similar household",
                Score = 72,
                Confidence = "Medium",
                Status = "Awaiting Review",
                Category = "Emergency shelter",
                Detected = "Yesterday",
                Description = "The requests have similar household information and nearby locations.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = "DUP-081",
                PrimaryCase = "REQ-2540",
                DuplicateCase = "REQ-2519",
                Factors = "Similar description · Matching category",
                Score = 64,
                Confidence = "Low",
                Status = "Not Duplicate",
                Category = "Food & water",
                Detected = "Yesterday",
                Description = "Review determined that these are separate valid assistance requests.",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-4)
            }
        };

        _duplicates.AddRange(items);
    }

    public Task<DuplicatesStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            var stats = new DuplicatesStatsDto
            {
                TotalMatches = _duplicates.Count,
                AwaitingReviewCount = _duplicates.Count(d => d.Status.Equals("Awaiting Review", StringComparison.OrdinalIgnoreCase)),
                HighConfidenceCount = _duplicates.Count(d => d.Confidence.Equals("High", StringComparison.OrdinalIgnoreCase)),
                ConfirmedCount = _duplicates.Count(d => d.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase)),
                DeferredCount = _duplicates.Count(d => d.Status.Equals("Deferred", StringComparison.OrdinalIgnoreCase)),
                NotDuplicateCount = _duplicates.Count(d => d.Status.Equals("Not Duplicate", StringComparison.OrdinalIgnoreCase))
            };

            return Task.FromResult(stats);
        }
    }

    public Task<PaginatedResult<DuplicateCaseItem>> GetDuplicatesAsync(DuplicateQueryDto query)
    {
        lock (_lock)
        {
            var queryable = _duplicates.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                queryable = queryable.Where(d =>
                    d.Id.ToLower().Contains(s) ||
                    d.PrimaryCase.ToLower().Contains(s) ||
                    d.DuplicateCase.ToLower().Contains(s) ||
                    d.Factors.ToLower().Contains(s) ||
                    d.Category.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(query.Similarity) && !query.Similarity.Equals("All similarity levels", StringComparison.OrdinalIgnoreCase))
            {
                if (query.Similarity.Equals("High", StringComparison.OrdinalIgnoreCase))
                    queryable = queryable.Where(d => d.Score >= 85);
                else if (query.Similarity.Equals("Medium", StringComparison.OrdinalIgnoreCase))
                    queryable = queryable.Where(d => d.Score >= 70 && d.Score < 85);
                else if (query.Similarity.Equals("Low", StringComparison.OrdinalIgnoreCase))
                    queryable = queryable.Where(d => d.Score < 70);
            }

            if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All review states", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(d => d.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Category) && !query.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(d => d.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.DateRange) && !query.DateRange.Equals("Date detected", StringComparison.OrdinalIgnoreCase))
            {
                if (query.DateRange.Equals("Today", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = queryable.Where(d => !d.Detected.Equals("Yesterday", StringComparison.OrdinalIgnoreCase));
                }
            }

            var list = queryable.OrderByDescending(d => d.Score).ToList();
            var totalCount = list.Count;
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 4 : query.PageSize;

            var items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Task.FromResult(new PaginatedResult<DuplicateCaseItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }

    public Task<DuplicateCaseItem?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _duplicates.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<DuplicateCaseItem?> UpdateStatusAsync(string id, UpdateDuplicateStatusRequest request)
    {
        lock (_lock)
        {
            var item = _duplicates.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.Status = request.Status;
                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    item.ReviewNotes = request.Notes;
                }
                return Task.FromResult<DuplicateCaseItem?>(item);
            }
            return Task.FromResult<DuplicateCaseItem?>(null);
        }
    }

    public Task<bool> MergeCasesAsync(MergeCasesRequest request)
    {
        lock (_lock)
        {
            var item = _duplicates.FirstOrDefault(d => d.Id.Equals(request.MatchId, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.Status = "Confirmed";
                item.ReviewNotes = string.IsNullOrWhiteSpace(request.ResolutionNotes)
                    ? $"Merged case {request.DuplicateCaseId} into primary {request.PrimaryCaseId}."
                    : request.ResolutionNotes;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public Task<byte[]> ExportCsvAsync(DuplicateQueryDto query)
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Match ID,Primary Case,Possible Duplicate,Similarity Factors,Score,Confidence,Status,Category,Detected");

            foreach (var item in _duplicates)
            {
                sb.AppendLine($"\"{item.Id}\",\"{item.PrimaryCase}\",\"{item.DuplicateCase}\",\"{item.Factors}\",\"{item.Score}%\",\"{item.Confidence}\",\"{item.Status}\",\"{item.Category}\",\"{item.Detected}\"");
            }

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }
}
