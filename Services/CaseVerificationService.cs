using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class CaseVerificationService : ICaseVerificationService
{
    private readonly object _lock = new();
    private readonly List<VerificationCaseItem> _cases = [];
    private int _verifiedTodayCount = 92;
    private int _infoRequestedCount = 18;
    private int _rejectedTodayCount = 4;

    public CaseVerificationService()
    {
        SeedInitialData();
    }

    private void SeedInitialData()
    {
        _cases.AddRange([
            new VerificationCaseItem
            {
                Id = "REQ-2841",
                Category = "Emergency shelter",
                Applicant = "D. Jayawardena - 6 people",
                Location = "Riverside, North Ward",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "94% confidence",
                Verification = "Evidence pending",
                Assigned = "You",
                Age = "18 min",
                Status = "Under Review",
                SubmissionAge = "18 min",
                EvidenceNotes = "Flood waters entered primary dwelling; photo evidence uploaded.",
                Documents = ["flood_water_damage.jpg", "grama_niladhari_cert.pdf"],
                CreatedAt = DateTime.UtcNow.AddMinutes(-18)
            },
            new VerificationCaseItem
            {
                Id = "REQ-2836",
                Category = "Food & water",
                Applicant = "M. Rasheed - 3 people",
                Location = "Eastbank, Sector 2",
                Priority = "High",
                AiPriority = "High",
                Confidence = "87% confidence",
                Verification = "Contact verified",
                Assigned = "Unassigned",
                Age = "49 min",
                Status = "Submitted",
                SubmissionAge = "49 min",
                EvidenceNotes = "Telephone contact verified with community elder.",
                Documents = ["id_card_scan.pdf"],
                CreatedAt = DateTime.UtcNow.AddMinutes(-49)
            },
            new VerificationCaseItem
            {
                Id = "REQ-2814",
                Category = "Medicine",
                Applicant = "R. Perera - 1 people",
                Location = "West District",
                Priority = "High",
                AiPriority = "Medium",
                Confidence = "74% confidence",
                Verification = "Needs information",
                Assigned = "Unassigned",
                Age = "2h 08m",
                Status = "Pending Info",
                SubmissionAge = "2h 08m",
                EvidenceNotes = "Prescription image is blurry. Requested clearer photograph.",
                Documents = ["prescription_blurry.jpg"],
                CreatedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(-8)
            },
            new VerificationCaseItem
            {
                Id = "REQ-2821",
                Category = "Rescue assistance",
                Applicant = "P. Nadarajah - 5 people",
                Location = "North Ward",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "96% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "1h 34m",
                Status = "Under Review",
                SubmissionAge = "1h 34m",
                EvidenceNotes = "Verified through disaster hotline coordinates.",
                Documents = ["gps_location_pin.png", "hotline_call_log.pdf"],
                CreatedAt = DateTime.UtcNow.AddHours(-1).AddMinutes(-34)
            },
            new VerificationCaseItem
            {
                Id = "REQ-2809",
                Category = "Emergency shelter",
                Applicant = "K. Alwis - 4 people",
                Location = "South Ward, Sector 3",
                Priority = "High",
                AiPriority = "High",
                Confidence = "89% confidence",
                Verification = "Evidence pending",
                Assigned = "Unassigned",
                Age = "35 min",
                Status = "Submitted",
                SubmissionAge = "35 min",
                EvidenceNotes = "Evacuation order issued for sub-district.",
                Documents = ["residence_proof.pdf"],
                CreatedAt = DateTime.UtcNow.AddMinutes(-35)
            },
            new VerificationCaseItem
            {
                Id = "REQ-2804",
                Category = "Food & water",
                Applicant = "S. Gunaratne - 2 people",
                Location = "Riverview, North Ward",
                Priority = "Medium",
                AiPriority = "Medium",
                Confidence = "81% confidence",
                Verification = "Contact verified",
                Assigned = "You",
                Age = "52 min",
                Status = "Under Review",
                SubmissionAge = "52 min",
                EvidenceNotes = "Identity verified via national ID registry.",
                Documents = ["national_identity_card.pdf"],
                CreatedAt = DateTime.UtcNow.AddMinutes(-52)
            },
            new VerificationCaseItem
            {
                Id = "REQ-2798",
                Category = "Medicine",
                Applicant = "F. Hameed - 2 people",
                Location = "Eastbank, Sector 4",
                Priority = "Critical",
                AiPriority = "Critical",
                Confidence = "93% confidence",
                Verification = "Evidence pending",
                Assigned = "Unassigned",
                Age = "42 min",
                Status = "Submitted",
                SubmissionAge = "42 min",
                EvidenceNotes = "Urgent asthma inhalers and nebulizer machine support.",
                Documents = ["hospital_clinic_record.jpg"],
                CreatedAt = DateTime.UtcNow.AddMinutes(-42)
            },
            new VerificationCaseItem
            {
                Id = "REQ-2792",
                Category = "Rescue assistance",
                Applicant = "N. Wickramasinghe - 3 people",
                Location = "West District",
                Priority = "High",
                AiPriority = "High",
                Confidence = "88% confidence",
                Verification = "Verified",
                Assigned = "You",
                Age = "2h 45m",
                Status = "Under Review",
                SubmissionAge = "2h 45m",
                EvidenceNotes = "Boat deployment authorized by emergency coordinator.",
                Documents = ["drone_surveillance_snap.jpg"],
                CreatedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(-45)
            }
        ]);
    }

    public Task<VerificationAnalyticsResponse> GetAnalyticsAsync()
    {
        lock (_lock)
        {
            var awaitingCount = _cases.Count(c => c.Verification == "Evidence pending");
            var criticalAwaiting = _cases.Count(c => c.Verification == "Evidence pending" && c.Priority == "Critical");

            var stats = new VerificationStatistics
            {
                AwaitingReview = awaitingCount,
                AwaitingReviewDesc = $"{criticalAwaiting} critical priority",
                VerifiedToday = _verifiedTodayCount,
                VerifiedTodayDesc = "96% passed",
                InformationRequested = _infoRequestedCount,
                InformationRequestedDesc = "Awaiting response",
                RejectedToday = _rejectedTodayCount,
                RejectedTodayDesc = "All with rationale",
                OldestPendingAge = "42 min"
            };

            var response = new VerificationAnalyticsResponse
            {
                Statistics = stats,
                TotalCases = _cases.Count,
                Cases = _cases.ToList(),
                LastUpdated = DateTime.UtcNow
            };

            return Task.FromResult(response);
        }
    }

    public Task<PaginatedResult<VerificationCaseItem>> GetCasesAsync(VerificationQueryDto query)
    {
        lock (_lock)
        {
            var filtered = _cases.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLowerInvariant();
                filtered = filtered.Where(c =>
                    c.Id.ToLowerInvariant().Contains(s) ||
                    c.Applicant.ToLowerInvariant().Contains(s) ||
                    c.Location.ToLowerInvariant().Contains(s) ||
                    c.Category.ToLowerInvariant().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(query.EvidenceState) && query.EvidenceState != "All evidence states")
            {
                filtered = filtered.Where(c => c.Verification.Equals(query.EvidenceState, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Priority) && query.Priority != "All priorities")
            {
                filtered = filtered.Where(c => c.Priority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Category) && query.Category != "All categories")
            {
                filtered = filtered.Where(c => c.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.AgeFilter) && query.AgeFilter != "Submission age")
            {
                if (query.AgeFilter.Contains("1 hour", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(c => !c.Age.Contains("h", StringComparison.OrdinalIgnoreCase));
                }
            }

            var totalCount = filtered.Count();
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Max(1, query.PageSize);
            var items = filtered
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PaginatedResult<VerificationCaseItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            return Task.FromResult(result);
        }
    }

    public Task<VerificationCaseItem?> GetCaseByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(item);
        }
    }

    public Task<VerificationCaseItem?> RecordDecisionAsync(string id, VerificationDecisionRequest request)
    {
        lock (_lock)
        {
            var item = _cases.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<VerificationCaseItem?>(null);

            var decision = request.Decision.Trim().ToLowerInvariant();

            if (decision == "verify" || decision == "approve")
            {
                item.Verification = "Verified";
                item.Status = "Verified";
                _verifiedTodayCount++;
            }
            else if (decision == "requestinfo" || decision == "needs info")
            {
                item.Verification = "Needs information";
                item.Status = "Pending Info";
                _infoRequestedCount++;
            }
            else if (decision == "reject")
            {
                item.Verification = "Rejected";
                item.Status = "Rejected";
                _rejectedTodayCount++;
            }

            if (!string.IsNullOrWhiteSpace(request.Rationale))
            {
                item.EvidenceNotes = request.Rationale;
            }

            if (!string.IsNullOrWhiteSpace(request.VerifiedBy))
            {
                item.Assigned = request.VerifiedBy;
            }

            return Task.FromResult<VerificationCaseItem?>(item);
        }
    }

    public async Task<byte[]> ExportVerificationCsvAsync(VerificationQueryDto query)
    {
        var result = await GetCasesAsync(new VerificationQueryDto
        {
            Search = query.Search,
            EvidenceState = query.EvidenceState,
            Priority = query.Priority,
            Category = query.Category,
            AgeFilter = query.AgeFilter,
            PageNumber = 1,
            PageSize = 1000
        });

        var sb = new StringBuilder();
        sb.AppendLine("Case ID,Category,Applicant,Location,Priority,AI Priority,Verification,Assigned,Age,Status");

        foreach (var c in result.Items)
        {
            sb.AppendLine($"\"{c.Id}\",\"{c.Category}\",\"{c.Applicant}\",\"{c.Location}\",\"{c.Priority}\",\"{c.AiPriority}\",\"{c.Verification}\",\"{c.Assigned}\",\"{c.Age}\",\"{c.Status}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
