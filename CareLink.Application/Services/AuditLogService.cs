using System.Collections.Concurrent;
using System.Text;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.AuditLogs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

namespace CareLink.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository? _repository;
    private readonly ConcurrentDictionary<string, AuditLog> _records = new(StringComparer.OrdinalIgnoreCase);
    private int _idSequence = 482200;

    public AuditLogService(IAuditLogRepository? repository = null)
    {
        _repository = repository;
        InitializeDefaultRecords();
    }

    #region Platform Administrative Audit Logs (HEAD)

    public Task<ApiResponse<PaginatedListDto<AuditLogDto>>> GetAuditLogsAsync(AuditLogQueryDto query)
    {
        var items = FilterAuditLogs(query);

        var totalCount = items.Count();
        var page = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var ordered = items.OrderByDescending(r => r.CreatedAt);

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

    public async Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(string id)
    {
        if (_records.TryGetValue(id, out var record))
        {
            return ApiResponse<AuditLogDto>.SuccessResult(MapToDto(record));
        }

        if (_repository != null)
        {
            var log = await _repository.GetByIdAsync(id);
            if (log != null)
            {
                return ApiResponse<AuditLogDto>.SuccessResponse(MapToDto(log));
            }
        }

        return ApiResponse<AuditLogDto>.FailureResult($"Audit log '{id}' not found.");
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

    public async Task<ApiResponse<AuditLogDto>> CreateAuditLogAsync(CreateAuditLogRequestDto request, CancellationToken cancellationToken = default)
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
        if (_repository != null)
        {
            await _repository.AddAsync(entity);
        }

        return ApiResponse<AuditLogDto>.SuccessResult(
            MapToDto(entity), "Audit log entry created successfully.");
    }

    public Task<(byte[] Bytes, AuditLogDto LogEntry)> ExportAuditLogAsync(ExportAuditLogRequestDto request, string actor)
    {
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

        var sb = new StringBuilder();
        sb.AppendLine("Event ID,Action,Actor,Timestamp,Associated Record,Previous to New,Rationale,Record Type");

        foreach (var r in exportList)
        {
            var row = string.Join(",",
                EscapeCsv(r.Id),
                EscapeCsv(r.Action),
                EscapeCsv(r.Actor),
                EscapeCsv(r.Timestamp),
                EscapeCsv(r.Record),
                EscapeCsv(r.Change),
                EscapeCsv(r.Rationale),
                EscapeCsv(r.Type)
            );
            sb.AppendLine(row);
        }

        var nextId = $"AUD-{Interlocked.Increment(ref _idSequence)}";
        var auditEntry = new AuditLog
        {
            Id = nextId,
            Action = "Audit log exported",
            Actor = actor,
            Record = "AUDIT_EXPORT",
            Change = $"Exported {exportList.Count} records (Scope: {request.Scope})",
            Rationale = request.Rationale.Trim(),
            Type = "Security",
            CreatedAt = DateTime.UtcNow,
            Environment = "Production"
        };
        _records[auditEntry.Id] = auditEntry;

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Task.FromResult((bytes, MapToDto(auditEntry)));
    }

    #endregion

    #region Resource Manager Audit Logs (Oshadi)

    public async Task<ApiResponse<List<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterParams? filters = null)
    {
        if (_repository != null)
        {
            var logs = await _repository.GetAllAsync(filters);
            var dtos = logs.Select(MapToDto).ToList();
            return ApiResponse<List<AuditLogDto>>.SuccessResponse(dtos, "Audit logs retrieved successfully.");
        }

        var query = _records.Values.AsEnumerable();
        if (filters != null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var s = filters.Search.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    x.Id.ToLowerInvariant().Contains(s) ||
                    x.Action.ToLowerInvariant().Contains(s) ||
                    x.Actor.ToLowerInvariant().Contains(s) ||
                    x.Record.ToLowerInvariant().Contains(s) ||
                    x.Change.ToLowerInvariant().Contains(s) ||
                    x.Rationale.ToLowerInvariant().Contains(s));
            }
            if (!string.IsNullOrWhiteSpace(filters.ActionType) &&
                !filters.ActionType.Equals("All action types", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.ActionType.Equals(filters.ActionType, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(filters.Actor) &&
                !filters.Actor.Equals("All users", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Actor.Equals(filters.Actor, StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query.OrderByDescending(x => x.CreatedAt).Select(MapToDto).ToList();
        return ApiResponse<List<AuditLogDto>>.SuccessResponse(list, "Audit logs retrieved successfully.");
    }

    public async Task<ApiResponse<AuditLogDto>> CreateAuditLogAsync(CreateAuditLogRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Action))
        {
            return ApiResponse<AuditLogDto>.ErrorResponse("Action is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Record))
        {
            return ApiResponse<AuditLogDto>.ErrorResponse("Record identifier is required.");
        }

        var now = DateTime.UtcNow;
        var dateStr = string.IsNullOrWhiteSpace(request.Date)
            ? now.ToString("yyyy-MM-dd")
            : request.Date;

        var randomCode = Random.Shared.Next(100000, 999999).ToString();

        var auditLog = new AuditLog
        {
            Id = $"AUD-{randomCode}",
            Action = request.Action.Trim(),
            Actor = string.IsNullOrWhiteSpace(request.Actor) ? "Daniel Perera" : request.Actor.Trim(),
            Timestamp = now.ToString("dd MMM · HH:mm"),
            Record = request.Record.Trim(),
            Change = request.Change.Trim(),
            Rationale = request.Rationale.Trim(),
            ActionType = string.IsNullOrWhiteSpace(request.ActionType) ? "Verification" : request.ActionType.Trim(),
            RecordType = string.IsNullOrWhiteSpace(request.RecordType) ? "System" : request.RecordType.Trim(),
            Date = dateStr,
            CreatedAt = now
        };

        _records[auditLog.Id] = auditLog;
        if (_repository != null)
        {
            await _repository.AddAsync(auditLog);
        }

        return ApiResponse<AuditLogDto>.SuccessResponse(MapToDto(auditLog), "Audit log recorded successfully.");
    }

    public async Task<ApiResponse<AuditLogSummaryDto>> GetSummaryAsync()
    {
        if (_repository != null)
        {
            var summary = await _repository.GetSummaryAsync();
            return ApiResponse<AuditLogSummaryDto>.SuccessResponse(summary, "Audit log summary retrieved successfully.");
        }

        var all = _records.Values.ToList();
        var today = DateTime.UtcNow.Date;
        var sum = new AuditLogSummaryDto
        {
            TotalLogs = all.Count,
            ApprovalsCount = all.Count(x => x.ActionType.Equals("Approval", StringComparison.OrdinalIgnoreCase)),
            DispatchesCount = all.Count(x => x.ActionType.Equals("Dispatch", StringComparison.OrdinalIgnoreCase)),
            InventoryCount = all.Count(x => x.RecordType.Equals("Inventory", StringComparison.OrdinalIgnoreCase)),
            TodayCount = all.Count(x => x.CreatedAt >= today)
        };
        return ApiResponse<AuditLogSummaryDto>.SuccessResponse(sum, "Audit log summary retrieved successfully.");
    }

    #endregion

    #region Helpers & Data Initialization

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
            Timestamp = r.Timestamp,
            CreatedAt = r.CreatedAt,
            Record = r.Record,
            Change = r.Change,
            Rationale = r.Rationale,
            Type = r.Type,
            ActionType = r.ActionType,
            RecordType = r.RecordType,
            Date = r.Date,
            Environment = r.Environment,
            IpAddress = r.IpAddress,
            Metadata = r.Metadata
        };
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
                ActionType = "User Management",
                RecordType = "User",
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
                ActionType = "User Management",
                RecordType = "User",
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
                ActionType = "Verification",
                RecordType = "Organisation",
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
                ActionType = "Configuration",
                RecordType = "System",
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
                ActionType = "Integration",
                RecordType = "Integration",
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
                Rationale = "Admin promotion",
                Type = "Role",
                ActionType = "Role",
                RecordType = "Role",
                Environment = "Production",
                IpAddress = "192.168.1.10"
            }
        };

        foreach (var r in seedList)
        {
            _records[r.Id] = r;
        }
    }

    #endregion
}
