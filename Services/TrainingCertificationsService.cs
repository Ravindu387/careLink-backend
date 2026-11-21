using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class TrainingCertificationsService : ITrainingCertificationsService
{
    private readonly object _lock = new();
    private readonly List<TrainingDto> _trainings = [];
    private readonly List<CertificateDto> _certificates = [];
    private int _trnCounter = 120;
    private int _certCounter = 2050;

    public TrainingCertificationsService()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        _trainings.AddRange([
            new TrainingDto
            {
                Id = "TRN-104",
                Name = "Flood Response Safety",
                Requirement = "Required",
                Duration = "45 min",
                Enrolled = 1986,
                Completion = 92,
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new TrainingDto
            {
                Id = "TRN-108",
                Name = "Safeguarding Essentials",
                Requirement = "Required",
                Duration = "60 min",
                Enrolled = 1742,
                Completion = 86,
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new TrainingDto
            {
                Id = "TRN-112",
                Name = "First Aid Refresher",
                Requirement = "Skill-based",
                Duration = "90 min",
                Enrolled = 486,
                Completion = 78,
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new TrainingDto
            {
                Id = "TRN-117",
                Name = "Radio Communications",
                Requirement = "Optional",
                Duration = "30 min",
                Enrolled = 328,
                Completion = 64,
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        ]);

        _certificates.AddRange([
            new CertificateDto
            {
                Id = "CERT-2048",
                Volunteer = "Musa Bello",
                Qualification = "Heavy Vehicle Licence",
                Expiry = "28 Jun 2025",
                Remaining = "6 days",
                Status = "Expiring Soon",
                DocumentUrl = "/documents/cert-2048.pdf",
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new CertificateDto
            {
                Id = "CERT-1992",
                Volunteer = "Elena Rossi",
                Qualification = "First Aid Level 2",
                Expiry = "12 Aug 2025",
                Remaining = "51 days",
                Status = "Valid",
                DocumentUrl = "/documents/cert-1992.pdf",
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new CertificateDto
            {
                Id = "CERT-1841",
                Volunteer = "Theo Martin",
                Qualification = "Rescue Operations",
                Expiry = "20 Jun 2025",
                Remaining = "Expired",
                Status = "Expired",
                DocumentUrl = "/documents/cert-1841.pdf",
                CreatedAt = DateTime.UtcNow.AddMonths(-12)
            }
        ]);
    }

    public Task<TrainingOverviewDto> GetOverviewAsync()
    {
        lock (_lock)
        {
            var overview = new TrainingOverviewDto
            {
                Stats = CalculateStats(),
                Trainings = _trainings.Select(CloneTraining).ToList(),
                Certificates = _certificates.Select(CloneCertificate).ToList()
            };

            return Task.FromResult(overview);
        }
    }

    public Task<List<TrainingDto>> GetTrainingsAsync(string? search = null, string? status = null, string? requirement = null)
    {
        lock (_lock)
        {
            var query = _trainings.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                query = query.Where(t =>
                    t.Id.ToLowerInvariant().Contains(s) ||
                    t.Name.ToLowerInvariant().Contains(s) ||
                    t.Requirement.ToLowerInvariant().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(requirement) && !requirement.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.Requirement.Equals(requirement, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(query.Select(CloneTraining).ToList());
        }
    }

    public Task<TrainingDto?> GetTrainingByIdAsync(string id)
    {
        lock (_lock)
        {
            var training = _trainings.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(training != null ? CloneTraining(training) : null);
        }
    }

    public Task<TrainingDto> CreateTrainingAsync(CreateTrainingRequest request)
    {
        lock (_lock)
        {
            var newId = $"TRN-{_trnCounter++}";
            var training = new TrainingDto
            {
                Id = newId,
                Name = request.Name.Trim(),
                Requirement = string.IsNullOrWhiteSpace(request.Requirement) ? "Required" : request.Requirement.Trim(),
                Duration = string.IsNullOrWhiteSpace(request.Duration) ? "45 min" : request.Duration.Trim(),
                Enrolled = request.Enrolled >= 0 ? request.Enrolled : 0,
                Completion = request.Completion >= 0 ? request.Completion : 0,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _trainings.Add(training);
            return Task.FromResult(CloneTraining(training));
        }
    }

    public Task<TrainingDto?> UpdateTrainingStatusAsync(string id, string status)
    {
        lock (_lock)
        {
            var training = _trainings.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (training == null)
            {
                return Task.FromResult<TrainingDto?>(null);
            }

            training.Status = status;
            return Task.FromResult<TrainingDto?>(CloneTraining(training));
        }
    }

    public Task<bool> DeleteTrainingAsync(string id)
    {
        lock (_lock)
        {
            var training = _trainings.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (training == null)
            {
                return Task.FromResult(false);
            }

            _trainings.Remove(training);
            return Task.FromResult(true);
        }
    }

    public Task<byte[]> ExportTrainingsCsvAsync(string? search = null, string? status = null)
    {
        lock (_lock)
        {
            var query = _trainings.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                query = query.Where(t =>
                    t.Id.ToLowerInvariant().Contains(s) ||
                    t.Name.ToLowerInvariant().Contains(s) ||
                    t.Requirement.ToLowerInvariant().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            var sb = new StringBuilder();
            sb.AppendLine("Course ID,Course,Requirement,Duration,Enrolled,Completion,Status");

            foreach (var t in query)
            {
                sb.AppendLine($"\"{EscapeCsv(t.Id)}\",\"{EscapeCsv(t.Name)}\",\"{EscapeCsv(t.Requirement)}\",\"{EscapeCsv(t.Duration)}\",{t.Enrolled},\"{t.Completion}%\",\"{EscapeCsv(t.Status)}\"");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return Task.FromResult(bytes);
        }
    }

    public Task<List<CertificateDto>> GetCertificatesAsync(string? search = null, string? status = null)
    {
        lock (_lock)
        {
            var query = _certificates.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    c.Id.ToLowerInvariant().Contains(s) ||
                    c.Volunteer.ToLowerInvariant().Contains(s) ||
                    c.Qualification.ToLowerInvariant().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(query.Select(CloneCertificate).ToList());
        }
    }

    public Task<CertificateDto?> GetCertificateByIdAsync(string id)
    {
        lock (_lock)
        {
            var cert = _certificates.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(cert != null ? CloneCertificate(cert) : null);
        }
    }

    public Task<CertificateDto> CreateCertificateAsync(CreateCertificateRequest request)
    {
        lock (_lock)
        {
            var newId = $"CERT-{_certCounter++}";
            var remaining = CalculateRemainingDays(request.Expiry);
            var status = DetermineStatus(remaining, request.Expiry);

            var cert = new CertificateDto
            {
                Id = newId,
                Volunteer = request.Volunteer.Trim(),
                Qualification = request.Qualification.Trim(),
                Expiry = request.Expiry.Trim(),
                Remaining = remaining,
                Status = status,
                DocumentUrl = !string.IsNullOrWhiteSpace(request.FileName) ? $"/documents/{request.FileName}" : "/documents/certificate.pdf",
                CreatedAt = DateTime.UtcNow
            };

            _certificates.Insert(0, cert);
            return Task.FromResult(CloneCertificate(cert));
        }
    }

    public Task<CertificateDto?> UpdateCertificateStatusAsync(string id, string status)
    {
        lock (_lock)
        {
            var cert = _certificates.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (cert == null)
            {
                return Task.FromResult<CertificateDto?>(null);
            }

            cert.Status = status;
            return Task.FromResult<CertificateDto?>(CloneCertificate(cert));
        }
    }

    public Task<bool> DeleteCertificateAsync(string id)
    {
        lock (_lock)
        {
            var cert = _certificates.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (cert == null)
            {
                return Task.FromResult(false);
            }

            _certificates.Remove(cert);
            return Task.FromResult(true);
        }
    }

    public Task<byte[]> ExportCertificatesCsvAsync(string? search = null, string? status = null)
    {
        lock (_lock)
        {
            var query = _certificates.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    c.Id.ToLowerInvariant().Contains(s) ||
                    c.Volunteer.ToLowerInvariant().Contains(s) ||
                    c.Qualification.ToLowerInvariant().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            var sb = new StringBuilder();
            sb.AppendLine("Certificate ID,Volunteer,Qualification,Expiry,Remaining,Status");

            foreach (var c in query)
            {
                sb.AppendLine($"\"{EscapeCsv(c.Id)}\",\"{EscapeCsv(c.Volunteer)}\",\"{EscapeCsv(c.Qualification)}\",\"{EscapeCsv(c.Expiry)}\",\"{EscapeCsv(c.Remaining)}\",\"{EscapeCsv(c.Status)}\"");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return Task.FromResult(bytes);
        }
    }

    private TrainingCertStatsDto CalculateStats()
    {
        var activeTrainings = _trainings.Where(t => t.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToList();
        var avgCompletion = activeTrainings.Count > 0 ? (int)Math.Round(activeTrainings.Average(t => t.Completion)) : 88;

        var validCount = _certificates.Count(c => c.Status.Equals("Valid", StringComparison.OrdinalIgnoreCase));
        var expiringSoonCount = _certificates.Count(c => c.Status.Equals("Expiring Soon", StringComparison.OrdinalIgnoreCase));
        var expiredCount = _certificates.Count(c => c.Status.Equals("Expired", StringComparison.OrdinalIgnoreCase));

        return new TrainingCertStatsDto
        {
            TrainingCompletionRate = $"{avgCompletion}%",
            CompletionSubtitle = "Mandatory modules",
            CertificationsValid = validCount > 0 ? (1840 + validCount).ToString("N0") : "1,842",
            ValidSubtitle = "Across verified volunteers",
            ExpiringSoon = (40 + expiringSoonCount).ToString(),
            ExpiringSubtitle = "Within 30 days",
            Expired = (17 + expiredCount).ToString(),
            ExpiredSubtitle = "Matching suspended"
        };
    }

    private static string CalculateRemainingDays(string expiryStr)
    {
        if (DateTime.TryParse(expiryStr, out var expiryDate))
        {
            var diff = (expiryDate.Date - DateTime.UtcNow.Date).TotalDays;
            if (diff < 0) return "Expired";
            if (diff == 0) return "Expires today";
            if (diff == 1) return "1 day";
            return $"{(int)diff} days";
        }
        return "Pending review";
    }

    private static string DetermineStatus(string remaining, string expiryStr)
    {
        if (remaining.Equals("Expired", StringComparison.OrdinalIgnoreCase)) return "Expired";
        if (DateTime.TryParse(expiryStr, out var expiryDate))
        {
            var diff = (expiryDate.Date - DateTime.UtcNow.Date).TotalDays;
            if (diff < 0) return "Expired";
            if (diff <= 30) return "Expiring Soon";
            return "Valid";
        }
        return "Valid";
    }

    private static TrainingDto CloneTraining(TrainingDto t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Requirement = t.Requirement,
        Duration = t.Duration,
        Enrolled = t.Enrolled,
        Completion = t.Completion,
        Status = t.Status,
        CreatedAt = t.CreatedAt
    };

    private static CertificateDto CloneCertificate(CertificateDto c) => new()
    {
        Id = c.Id,
        Volunteer = c.Volunteer,
        Qualification = c.Qualification,
        Expiry = c.Expiry,
        Remaining = c.Remaining,
        Status = c.Status,
        DocumentUrl = c.DocumentUrl,
        CreatedAt = c.CreatedAt
    };

    private static string EscapeCsv(string val) => (val ?? "").Replace("\"", "\"\"");
}
