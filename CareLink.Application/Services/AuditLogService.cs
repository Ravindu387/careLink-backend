using System.Collections.Concurrent;
using System.Text;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

namespace CareLink.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ConcurrentDictionary<string, AuditLog> _records = new(StringComparer.OrdinalIgnoreCase);
    private int _idSequence = 482200;

    public AuditLogService()
    {
        InitializeDefaultRecords();
    }

    private void InitializeDefaultRecords()
    {
        var now = DateTime.UtcNow;

        var seedList = new List<AuditLog>
        {
            new()
            {
                Id = "AUD-482194",
                Action = "User role updated",
                Actor = "Alex Kimani",
                CreatedAt = now.AddMinutes(-9),
                Record = "USR-10458",
                Change = "Partner Officer → Coordinator",
                Rationale = "Approved request",
                Type = "User",
                Environment = "Production",
                IpAddress = "192.168.1.10"
            },
            new()
            {
                Id = "AUD-482188",
                Action = "Account disabled",
                Actor = "Marta Silva",
                CreatedAt = now.AddMinutes(-32),
                Record = "USR-10312",
                Change = "Active → Disabled",
                Rationale = "Security policy",
                Type = "User",
                Environment = "Production",
                IpAddress = "192.168.1.15"
            },
            new()
            {
                Id = "AUD-482174",
                Action = "Organisation verified",
                Actor = "Alex Kimani",
                CreatedAt = now.AddHours(-1).AddMinutes(-8),
                Record = "ORG-042",
                Change = "Pending → Verified",
                Rationale = "Documents reviewed",
                Type = "Organisation",
                Environment = "Production",
                IpAddress = "192.168.1.10"
            },
            new()
            {
                Id = "AUD-482161",
                Action = "System setting changed",
                Actor = "System Service",
                CreatedAt = now.AddHours(-1).AddMinutes(-50),
                Record = "SECURITY",
                Change = "30 min → 20 min",
                Rationale = "Policy deployment",
                Type = "System",
                Environment = "Production",
                IpAddress = "127.0.0.1"
            },
            new()
            {
                Id = "AUD-482142",
                Action = "Integration retried",
                Actor = "Devon Chen",
                CreatedAt = now.AddHours(-2).AddMinutes(-17),
                Record = "INT-004",
                Change = "Failed → Healthy",
                Rationale = "Manual retry",
                Type = "Integration",
                Environment = "Production",
                IpAddress = "192.168.1.22"
            },
            new()
            {
                Id = "AUD-482130",
                Action = "Permission updated",
                Actor = "Alex Kimani",
                CreatedAt = now.AddHours(-2).AddMinutes(-40),
                Record = "ROLE-006",
                Change = "View only → Manage",
                Rationale = "Role review",
                Type = "Role",
                Environment = "Production",
                IpAddress = "192.168.1.10"
            },
            new()
            {
                Id = "AUD-482119",
                Action = "Shelter record created",
                Actor = "Marta Silva",
                CreatedAt = now.AddHours(-3).AddMinutes(-5),
                Record = "SHL-091",
                Change = "New record",
                Rationale = "Operational update",
                Type = "Shelter",
                Environment = "Production",
                IpAddress = "192.168.1.15"
            },
            new()
            {
                Id = "AUD-482105",
                Action = "Report exported",
                Actor = "Devon Chen",
                CreatedAt = now.AddHours(-3).AddMinutes(-48),
                Record = "RPT-221",
                Change = "Generated → Exported",
                Rationale = "Requested report",
                Type = "Report",
                Environment = "Production",
                IpAddress = "192.168.1.22"
            },
            new()
            {
                Id = "AUD-482090",
                Action = "Crisis zone boundary modified",
                Actor = "Alex Kimani",
                CreatedAt = now.AddHours(-5).AddMinutes(-12),
                Record = "ZONE-04",
                Change = "Warning → Red Evacuation",
                Rationale = "Water levels exceeded threshold",
                Type = "System",
                Environment = "Production",
                IpAddress = "192.168.1.10"
            },
            new()
            {
                Id = "AUD-482081",
                Action = "Volunteer credential issued",
                Actor = "Marta Silva",
                CreatedAt = now.AddHours(-8).AddMinutes(-30),
                Record = "VOL-2918",
                Change = "Trainee → Level 2 First Responder",
                Rationale = "Certification passed",
                Type = "User",
                Environment = "Production",
                IpAddress = "192.168.1.15"
            },
            new()
            {
                Id = "AUD-482065",
                Action = "Food ration inventory disbursed",
                Actor = "Devon Chen",
                CreatedAt = now.AddHours(-18),
                Record = "RES-402",
                Change = "500 units → 120 units",
                Rationale = "Emergency relief dispatch",
                Type = "Shelter",
                Environment = "Production",
                IpAddress = "192.168.1.22"
            },
            new()
            {
                Id = "AUD-482052",
                Action = "AI triage recommendation overridden",
                Actor = "Alex Kimani",
                CreatedAt = now.AddDays(-1).AddHours(-2),
                Record = "REQ-2841",
                Change = "High → Critical",
                Rationale = "Immediate life hazard confirmed",
                Type = "System",
                Environment = "Production",
                IpAddress = "192.168.1.10"
            }
        };

        foreach (var item in seedList)
        {
            _records[item.Id] = item;
        }
    }

    public Task<ApiResponse<PaginatedListDto<AuditLogDto>>> GetAuditLogsAsync(AuditLogQueryDto query)
    {
        var items = FilterAuditLogs(query);
        var ordered = items.OrderByDescending(r => r.CreatedAt).ToList();

        var totalCount = ordered.Count;
        var page = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var pagedItems = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        var result = new PaginatedListDto<AuditLogDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return Task.FromResult(ApiResponse<PaginatedListDto<AuditLogDto>>.SuccessResult(result));
    }

    public Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(string id)
    {
        if (_records.TryGetValue(id, out var record))
        {
            return Task.FromResult(ApiResponse<AuditLogDto>.SuccessResult(MapToDto(record)));
        }

        return Task.FromResult(ApiResponse<AuditLogDto>.FailureResult($"Audit log '{id}' not found."));
    }

    public Task<ApiResponse<AuditLogStatsDto>> GetStatsAsync()
    {
        var all = _records.Values.ToList();
        var today = DateTime.UtcNow.Date;

        var stats = new AuditLogStatsDto
        {
            TotalRecords = all.Count,
            RetentionPeriod = "7 years",
            UniqueActors = all.Select(r => r.Actor).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            ActionsToday = all.Count(r => r.CreatedAt >= today),
            ActionTypes = all.Select(r => r.Action).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(a => a).ToList(),
            RecordTypes = all.Select(r => r.Type).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList(),
            Actors = all.Select(r => r.Actor).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(a => a).ToList()
        };

        return Task.FromResult(ApiResponse<AuditLogStatsDto>.SuccessResult(stats));
    }

    public Task<ApiResponse<IReadOnlyList<string>>> GetActionTypesAsync()
    {
        var list = _records.Values
            .Select(r => r.Action)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        return Task.FromResult(ApiResponse<IReadOnlyList<string>>.SuccessResult(list));
    }

    public Task<ApiResponse<IReadOnlyList<string>>> GetRecordTypesAsync()
    {
        var list = _records.Values
            .Select(r => r.Type)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        return Task.FromResult(ApiResponse<IReadOnlyList<string>>.SuccessResult(list));
    }

    public Task<ApiResponse<IReadOnlyList<string>>> GetActorsAsync()
    {
        var list = _records.Values
            .Select(r => r.Actor)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        return Task.FromResult(ApiResponse<IReadOnlyList<string>>.SuccessResult(list));
    }

    public Task<ApiResponse<AuditLogDto>> CreateAuditLogAsync(CreateAuditLogRequestDto request)
    {
        var nextId = $"AUD-{Interlocked.Increment(ref _idSequence)}";
        var entity = new AuditLog
        {
            Id = nextId,
            Action = request.Action.Trim(),
            Actor = request.Actor.Trim(),
            ActorId = request.ActorId?.Trim(),
            Record = request.Record.Trim(),
            Change = request.Change.Trim(),
            Rationale = request.Rationale.Trim(),
            Type = string.IsNullOrWhiteSpace(request.Type) ? "System" : request.Type.Trim(),
            CreatedAt = DateTime.UtcNow,
            Environment = string.IsNullOrWhiteSpace(request.Environment) ? "Production" : request.Environment.Trim(),
            IpAddress = request.IpAddress?.Trim(),
            Metadata = request.Metadata
        };

        _records[entity.Id] = entity;

        return Task.FromResult(ApiResponse<AuditLogDto>.SuccessResult(
            MapToDto(entity), "Audit log entry created successfully."));
    }

    public Task<(byte[] Bytes, AuditLogDto LogEntry)> ExportAuditLogAsync(ExportAuditLogRequestDto request, string actor)
    {
        // 1. Determine which records to export based on scope
        IEnumerable<AuditLog> recordsToExport;

        if (request.Scope.Equals("Selected record", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(request.SelectedRecordId) &&
            _records.TryGetValue(request.SelectedRecordId, out var selected))
        {
            recordsToExport = [selected];
        }
        else if (request.Scope.Equals("All available records", StringComparison.OrdinalIgnoreCase))
        {
            recordsToExport = _records.Values;
        }
        else
        {
            // Current filtered results
            var query = new AuditLogQueryDto
            {
                Search = request.Search,
                Action = request.Action,
                Actor = request.Actor,
                Type = request.Type,
                DateFilter = request.DateFilter
            };
            recordsToExport = FilterAuditLogs(query);
        }

        var exportList = recordsToExport.OrderByDescending(r => r.CreatedAt).ToList();

        // 2. Generate CSV
        var sb = new StringBuilder();
        sb.AppendLine("Event ID,Action,Actor,Timestamp,Associated Record,Previous to New,Rationale,Record Type");

        foreach (var r in exportList)
        {
            var row = string.Join(",",
                EscapeCsv(r.Id),
                EscapeCsv(r.Action),
                EscapeCsv(r.Actor),
                EscapeCsv(FormatTimestamp(r.CreatedAt)),
                EscapeCsv(r.Record),
                EscapeCsv(r.Change),
                EscapeCsv(r.Rationale),
                EscapeCsv(r.Type)
            );
            sb.AppendLine(row);
        }

        // 3. Immutably record the export action into the audit trail
        var nextId = $"AUD-{Interlocked.Increment(ref _idSequence)}";
        var exportEntry = new AuditLog
        {
            Id = nextId,
            Action = "Audit log exported",
            Actor = string.IsNullOrWhiteSpace(actor) ? "System Administrator" : actor,
            Record = string.IsNullOrWhiteSpace(request.SelectedRecordId) ? "AUDIT-EXPORT" : request.SelectedRecordId,
            Change = "Export requested → Completed",
            Rationale = request.Rationale.Trim(),
            Type = "Report",
            CreatedAt = DateTime.UtcNow,
            Environment = string.IsNullOrWhiteSpace(request.Environment) ? "Production" : request.Environment.Trim(),
            Metadata = $"Scope: {request.Scope}; Count: {exportList.Count}"
        };

        _records[exportEntry.Id] = exportEntry;

        return Task.FromResult((Encoding.UTF8.GetBytes(sb.ToString()), MapToDto(exportEntry)));
    }

    private IEnumerable<AuditLog> FilterAuditLogs(AuditLogQueryDto query)
    {
        var items = _records.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var q = query.Search.Trim().ToLowerInvariant();
            items = items.Where(r =>
                r.Id.ToLowerInvariant().Contains(q) ||
                r.Action.ToLowerInvariant().Contains(q) ||
                r.Actor.ToLowerInvariant().Contains(q) ||
                r.Record.ToLowerInvariant().Contains(q) ||
                r.Change.ToLowerInvariant().Contains(q) ||
                r.Rationale.ToLowerInvariant().Contains(q) ||
                r.Type.ToLowerInvariant().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(query.Action) &&
            !query.Action.Equals("All action types", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(r => r.Action.Equals(query.Action, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Actor) &&
            !query.Actor.Equals("All users", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(r => r.Actor.Equals(query.Actor, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Type) &&
            !query.Type.Equals("All record types", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(r => r.Type.Equals(query.Type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.DateFilter) &&
            !query.DateFilter.Equals("All time", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.UtcNow;
            if (query.DateFilter.Equals("Last 24 hours", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(r => r.CreatedAt >= now.AddHours(-24));
            }
            else if (query.DateFilter.Equals("Last 7 days", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(r => r.CreatedAt >= now.AddDays(-7));
            }
            else if (query.DateFilter.Equals("Last 30 days", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(r => r.CreatedAt >= now.AddDays(-30));
            }
        }

        return items;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return $"\"{value}\"";
    }

    private static AuditLogDto MapToDto(AuditLog r)
    {
        return new AuditLogDto
        {
            Id = r.Id,
            Action = r.Action,
            Actor = r.Actor,
            ActorId = r.ActorId,
            Timestamp = FormatTimestamp(r.CreatedAt),
            CreatedAt = r.CreatedAt,
            Record = r.Record,
            Change = r.Change,
            Rationale = r.Rationale,
            Type = r.Type,
            Environment = r.Environment,
            IpAddress = r.IpAddress,
            Metadata = r.Metadata
        };
    }

    private static string FormatTimestamp(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 2) return "Just now";
        return dt.ToString("dd MMM · HH:mm");
    }
}
