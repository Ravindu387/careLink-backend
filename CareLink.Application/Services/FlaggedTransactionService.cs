using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class FlaggedTransactionService : IFlaggedTransactionService
{
    private readonly IFlaggedTransactionRepository _repository;

    public FlaggedTransactionService(IFlaggedTransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IEnumerable<FlaggedTransactionDto> Items, int TotalCount)> GetPagedAsync(FlaggedTransactionQueryParameters parameters)
    {
        var (items, total) = await _repository.GetPagedAsync(parameters);
        return (items.Select(MapToDto), total);
    }

    public async Task<IEnumerable<FlaggedTransactionDto>> GetAllAsync()
    {
        var all = await _repository.GetAllAsync();
        return all.Select(MapToDto);
    }

    public async Task<FlaggedTransactionSummaryDto> GetSummaryAsync()
    {
        var metrics = await _repository.GetSummaryMetricsAsync();
        return new FlaggedTransactionSummaryDto
        {
            TotalCount = metrics.TotalCount,
            OpenFlags = metrics.OpenFlags,
            HighPriority = metrics.HighPriority,
            UnderReview = metrics.UnderReview,
            Resolved = metrics.Resolved,
            HighPriorityCombinedAmount = metrics.HighPriorityCombinedAmount
        };
    }

    public async Task<FlaggedTransactionDto?> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<FlaggedTransactionDto> CreateAsync(CreateFlaggedTransactionDto dto)
    {
        var id = await _repository.GenerateNextIdAsync();
        var now = DateTime.UtcNow;
        var flaggedDisplay = now.ToString("dd MMM · HH:mm");

        // Infer RecordType from Record prefix
        var recordType = dto.RecordType;
        if (string.IsNullOrWhiteSpace(recordType))
        {
            recordType = dto.Record.StartsWith("DON") ? "Donations"
                : dto.Record.StartsWith("EXP") ? "Expenditure"
                : dto.Record.StartsWith("ALL") ? "Allocations"
                : "Other";
        }

        var amountDisplay = "$" + dto.Amount.ToString("N0");

        var entity = new FlaggedTransaction
        {
            Id = id,
            Record = dto.Record.Trim(),
            Reason = dto.Reason.Trim(),
            Amount = dto.Amount,
            AmountDisplay = amountDisplay,
            Party = dto.Party.Trim(),
            Severity = dto.Severity,
            Flagged = flaggedDisplay,
            FlaggedAt = now,
            Status = "Investigation Required",
            Description = dto.Description.Trim(),
            Investigator = string.IsNullOrWhiteSpace(dto.Investigator) ? "Not assigned" : dto.Investigator.Trim(),
            RecordType = recordType,
            ResolutionNotes = string.Empty,
            ResolvedAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _repository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<FlaggedTransactionDto?> UpdateStatusAsync(string id, UpdateFlaggedTransactionStatusDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Status = dto.Status;

        if (!string.IsNullOrWhiteSpace(dto.Investigator))
            entity.Investigator = dto.Investigator;

        if (!string.IsNullOrWhiteSpace(dto.ResolutionNotes))
            entity.ResolutionNotes = dto.ResolutionNotes;

        if (dto.Status == "Resolved")
            entity.ResolvedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<FlaggedTransactionDto?> AssignInvestigatorAsync(string id, string investigator)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Investigator = investigator.Trim();
        if (entity.Status == "Investigation Required")
            entity.Status = "Under Review";

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static FlaggedTransactionDto MapToDto(FlaggedTransaction f) => new()
    {
        Id = f.Id,
        Record = f.Record,
        Reason = f.Reason,
        Amount = f.Amount,
        AmountDisplay = f.AmountDisplay,
        Party = f.Party,
        Severity = f.Severity,
        Flagged = f.Flagged,
        FlaggedAt = f.FlaggedAt,
        Status = f.Status,
        Description = f.Description,
        Investigator = f.Investigator,
        RecordType = f.RecordType,
        ResolutionNotes = f.ResolutionNotes,
        ResolvedAt = f.ResolvedAt,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };
}
