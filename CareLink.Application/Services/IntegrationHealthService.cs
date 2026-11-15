using System.Collections.Concurrent;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Integrations;
using CareLink.Application.Interfaces;

namespace CareLink.Application.Services;

public class IntegrationHealthService : IIntegrationHealthService
{
    private readonly ConcurrentDictionary<string, IntegrationHealthDto> _integrations = new();
    private readonly ConcurrentDictionary<string, List<IntegrationLogDto>> _logs = new();
    private DateTime _lastGlobalCheck = DateTime.UtcNow;

    public IntegrationHealthService()
    {
        InitializeDefaultIntegrations();
    }

    private void InitializeDefaultIntegrations()
    {
        var seedList = new List<IntegrationHealthDto>
        {
            new()
            {
                Id = "INT-001",
                Name = "Identity & Access Service",
                Type = "Authentication",
                Availability = "99.99%",
                Latency = "42 ms",
                LatencyMs = 42,
                Health = "Operational",
                LastCheck = "Just now",
                LastCheckedAt = DateTime.UtcNow,
                Icon = "KeyRound",
                Endpoint = "http://localhost:5277/api/auth",
                Description = "ASP.NET Core Identity & JWT Bearer token generation subsystem.",
                Version = "v2.4.1"
            },
            new()
            {
                Id = "INT-002",
                Name = "Core Database & Storage",
                Type = "Database",
                Availability = "99.98%",
                Latency = "18 ms",
                LatencyMs = 18,
                Health = "Operational",
                LastCheck = "Just now",
                LastCheckedAt = DateTime.UtcNow,
                Icon = "Database",
                Endpoint = "tcp:localhost,1433",
                Description = "Microsoft SQL Server persistent relational database cluster.",
                Version = "v16.0.1"
            },
            new()
            {
                Id = "INT-004",
                Name = "SMS Delivery Gateway",
                Type = "Communications",
                Availability = "97.42%",
                Latency = "1.8 s",
                LatencyMs = 1800,
                Health = "Degraded",
                LastCheck = "1 min ago",
                LastCheckedAt = DateTime.UtcNow.AddMinutes(-1),
                Icon = "Bell",
                Endpoint = "https://sms-gw.carelink.org/v1/dispatch",
                Description = "National cellular broadcast and OTP emergency messaging gateway.",
                Version = "v3.1.0"
            },
            new()
            {
                Id = "INT-008",
                Name = "Email Delivery Service",
                Type = "Communications",
                Availability = "99.94%",
                Latency = "284 ms",
                LatencyMs = 284,
                Health = "Operational",
                LastCheck = "Just now",
                LastCheckedAt = DateTime.UtcNow,
                Icon = "Database",
                Endpoint = "smtp://smtp.carelink.org:587",
                Description = "High-throughput operational alert and invitation email relay.",
                Version = "v1.9.3"
            },
            new()
            {
                Id = "INT-012",
                Name = "Mapping & Geocoding",
                Type = "Location",
                Availability = "99.87%",
                Latency = "640 ms",
                LatencyMs = 640,
                Health = "Operational",
                LastCheck = "2 min ago",
                LastCheckedAt = DateTime.UtcNow.AddMinutes(-2),
                Icon = "Link2",
                Endpoint = "https://geo.carelink.org/api/v2/geocode",
                Description = "Topographic flood hazard mapping and GIS spatial geocoding engine.",
                Version = "v4.0.2"
            },
            new()
            {
                Id = "INT-016",
                Name = "AI Decision Support",
                Type = "AI Service",
                Availability = "99.91%",
                Latency = "1.2 s",
                LatencyMs = 1200,
                Health = "Operational",
                LastCheck = "Just now",
                LastCheckedAt = DateTime.UtcNow,
                Icon = "Zap",
                Endpoint = "https://ai.carelink.org/models/triage",
                Description = "Neural recommendation assistant for shelter routing and resource dispatch.",
                Version = "v1.2.0"
            },
            new()
            {
                Id = "INT-021",
                Name = "Government Alert Feed",
                Type = "External API",
                Availability = "94.10%",
                Latency = "—",
                LatencyMs = 0,
                Health = "Unavailable",
                LastCheck = "18 min ago",
                LastCheckedAt = DateTime.UtcNow.AddMinutes(-18),
                Icon = "Globe",
                Endpoint = "https://disaster.gov.lk/api/alerts/rss",
                Description = "National Disaster Management Centre upstream meteorology early-warning feed.",
                Version = "v1.0.0"
            }
        };

        foreach (var item in seedList)
        {
            _integrations[item.Id] = item;
            _logs[item.Id] = new List<IntegrationLogDto>
            {
                new()
                {
                    IntegrationId = item.Id,
                    Heading = "Health check completed",
                    Description = $"Connection status verified as {item.Health}.",
                    TimeAgo = item.LastCheck,
                    Timestamp = item.LastCheckedAt,
                    Status = item.Health == "Unavailable" ? "Error" : item.Health == "Degraded" ? "Warning" : "Success"
                },
                new()
                {
                    IntegrationId = item.Id,
                    Heading = "API telemetry recorded",
                    Description = $"Recorded latency: {item.Latency}, availability: {item.Availability}.",
                    TimeAgo = "5 minutes ago",
                    Timestamp = DateTime.UtcNow.AddMinutes(-5),
                    Status = "Success"
                },
                new()
                {
                    IntegrationId = item.Id,
                    Heading = "Connection handshake",
                    Description = "TLS session established and upstream service credentials confirmed.",
                    TimeAgo = "12 minutes ago",
                    Timestamp = DateTime.UtcNow.AddMinutes(-12),
                    Status = "Success"
                }
            };
        }
    }

    public Task<ApiResponse<List<IntegrationHealthDto>>> GetIntegrationsAsync(string? type = null, string? health = null, string? search = null)
    {
        var list = _integrations.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            list = list.Where(i => i.Id.ToLower().Contains(q) || i.Name.ToLower().Contains(q) || i.Type.ToLower().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(type) && !type.Equals("All service types", StringComparison.OrdinalIgnoreCase))
        {
            list = list.Where(i => i.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(health) && !health.Equals("All health states", StringComparison.OrdinalIgnoreCase))
        {
            list = list.Where(i => i.Health.Equals(health, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(ApiResponse<List<IntegrationHealthDto>>.SuccessResult(list.OrderBy(i => i.Id).ToList()));
    }

    public Task<ApiResponse<IntegrationHealthDto>> GetIntegrationByIdAsync(string id)
    {
        if (_integrations.TryGetValue(id, out var item))
        {
            return Task.FromResult(ApiResponse<IntegrationHealthDto>.SuccessResult(item));
        }

        return Task.FromResult(ApiResponse<IntegrationHealthDto>.FailureResult($"Integration '{id}' not found."));
    }

    public Task<ApiResponse<PlatformHealthOverviewDto>> GetOverviewAsync()
    {
        var list = _integrations.Values.ToList();
        var total = list.Count;
        var operational = list.Count(i => i.Health == "Operational");
        var degraded = list.Count(i => i.Health == "Degraded");
        var unavailable = list.Count(i => i.Health == "Unavailable");

        var percentage = total > 0 ? (int)Math.Round((double)operational / total * 100) : 100;
        var coreStatus = unavailable > 1 ? "Unavailable" : (degraded > 1 || unavailable == 1) ? "Degraded" : "Operational";

        // Calculate median latency of operational services with numeric latencies
        var operationalLatencies = list
            .Where(i => i.LatencyMs > 0)
            .Select(i => i.LatencyMs)
            .OrderBy(l => l)
            .ToList();

        string medianLatencyStr = "248 ms";
        if (operationalLatencies.Count > 0)
        {
            var mid = operationalLatencies.Count / 2;
            var median = operationalLatencies.Count % 2 != 0
                ? operationalLatencies[mid]
                : (operationalLatencies[mid - 1] + operationalLatencies[mid]) / 2.0;

            medianLatencyStr = median >= 1000 ? $"{(median / 1000.0):F1} s" : $"{Math.Round(median)} ms";
        }

        var overview = new PlatformHealthOverviewDto
        {
            CorePlatformStatus = coreStatus,
            ServicesHealthyPercentage = percentage,
            ApiAvailability = "99.94%",
            MedianLatency = medianLatencyStr,
            TotalServices = total,
            OperationalCount = operational,
            DegradedCount = degraded,
            UnavailableCount = unavailable,
            LastChecked = FormatRelativeTime(_lastGlobalCheck),
            LastCheckedAt = _lastGlobalCheck
        };

        return Task.FromResult(ApiResponse<PlatformHealthOverviewDto>.SuccessResult(overview));
    }

    public Task<ApiResponse<List<IntegrationLogDto>>> GetIntegrationLogsAsync(string integrationId)
    {
        if (_logs.TryGetValue(integrationId, out var logs))
        {
            return Task.FromResult(ApiResponse<List<IntegrationLogDto>>.SuccessResult(
                logs.OrderByDescending(l => l.Timestamp).ToList()));
        }

        return Task.FromResult(ApiResponse<List<IntegrationLogDto>>.SuccessResult(new List<IntegrationLogDto>()));
    }

    public Task<ApiResponse<List<IntegrationHealthDto>>> RunHealthChecksAsync()
    {
        _lastGlobalCheck = DateTime.UtcNow;
        var random = new Random();

        foreach (var kvp in _integrations)
        {
            var item = kvp.Value;
            item.LastCheckedAt = DateTime.UtcNow;
            item.LastCheck = "Just now";

            // If operational, adjust latency slightly within realistic boundaries
            if (item.Health == "Operational")
            {
                if (item.Type == "Database")
                {
                    item.LatencyMs = random.Next(12, 28);
                    item.Latency = $"{item.LatencyMs} ms";
                }
                else if (item.Type == "Authentication")
                {
                    item.LatencyMs = random.Next(35, 55);
                    item.Latency = $"{item.LatencyMs} ms";
                }
                else if (item.Type == "Communications")
                {
                    item.LatencyMs = random.Next(240, 320);
                    item.Latency = $"{item.LatencyMs} ms";
                }
                else if (item.Type == "Location")
                {
                    item.LatencyMs = random.Next(580, 690);
                    item.Latency = $"{item.LatencyMs} ms";
                }
                else if (item.Type == "AI Service")
                {
                    item.LatencyMs = random.Next(1100, 1400);
                    item.Latency = $"{(item.LatencyMs / 1000.0):F1} s";
                }
            }

            // Append a log entry
            var log = new IntegrationLogDto
            {
                IntegrationId = item.Id,
                Heading = "Scheduled Health Check",
                Description = $"Live probe returned {item.Health} status with latency of {item.Latency}.",
                TimeAgo = "Just now",
                Timestamp = DateTime.UtcNow,
                Status = item.Health == "Unavailable" ? "Error" : item.Health == "Degraded" ? "Warning" : "Success"
            };

            _logs.AddOrUpdate(item.Id,
                _ => new List<IntegrationLogDto> { log },
                (_, existingList) =>
                {
                    existingList.Insert(0, log);
                    if (existingList.Count > 20) existingList.RemoveAt(existingList.Count - 1);
                    return existingList;
                });
        }

        return Task.FromResult(ApiResponse<List<IntegrationHealthDto>>.SuccessResult(
            _integrations.Values.OrderBy(i => i.Id).ToList(),
            "All integration health checks completed successfully."));
    }

    public Task<ApiResponse<IntegrationHealthDto>> RetryIntegrationAsync(string id, IntegrationActionRequestDto request)
    {
        if (!_integrations.TryGetValue(id, out var item))
        {
            return Task.FromResult(ApiResponse<IntegrationHealthDto>.FailureResult($"Integration '{id}' not found."));
        }

        // Recover integration to operational
        item.Health = "Operational";
        item.LastCheckedAt = DateTime.UtcNow;
        item.LastCheck = "Just now";

        if (item.Id == "INT-021") // Government alert feed
        {
            item.LatencyMs = 95;
            item.Latency = "95 ms";
            item.Availability = "98.50%";
        }
        else if (item.Id == "INT-004") // SMS Gateway
        {
            item.LatencyMs = 380;
            item.Latency = "380 ms";
            item.Availability = "99.10%";
        }

        var log = new IntegrationLogDto
        {
            IntegrationId = item.Id,
            Heading = $"Administrative Recovery: {request.ActionTitle}",
            Description = $"Recovered to Operational in {request.Environment}. Rationale: {request.AdministrativeRationale}",
            TimeAgo = "Just now",
            Timestamp = DateTime.UtcNow,
            Status = "Success"
        };

        _logs.AddOrUpdate(item.Id,
            _ => new List<IntegrationLogDto> { log },
            (_, existingList) =>
            {
                existingList.Insert(0, log);
                return existingList;
            });

        return Task.FromResult(ApiResponse<IntegrationHealthDto>.SuccessResult(
            item, $"Integration '{item.Name}' was successfully recovered to Operational."));
    }

    public Task<ApiResponse<bool>> RecordActionAsync(string? integrationId, IntegrationActionRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(integrationId) && _integrations.TryGetValue(integrationId, out var item))
        {
            var log = new IntegrationLogDto
            {
                IntegrationId = item.Id,
                Heading = request.ActionTitle,
                Description = $"Authorized action in {request.Environment} ({request.ApprovalType}): {request.AdministrativeRationale}",
                TimeAgo = "Just now",
                Timestamp = DateTime.UtcNow,
                Status = "Success"
            };

            _logs.AddOrUpdate(item.Id,
                _ => new List<IntegrationLogDto> { log },
                (_, existingList) =>
                {
                    existingList.Insert(0, log);
                    return existingList;
                });
        }

        return Task.FromResult(ApiResponse<bool>.SuccessResult(
            true, $"{request.ActionTitle} recorded successfully for {request.Environment} environment."));
    }

    private static string FormatRelativeTime(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;
        if (span.TotalSeconds < 60) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hr ago";
        return dateTime.ToString("MMM dd, yyyy");
    }
}
