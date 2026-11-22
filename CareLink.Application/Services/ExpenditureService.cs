using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class ExpenditureService : IExpenditureService
{
    private readonly IExpenditureRepository _repository;

    public ExpenditureService(IExpenditureRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IEnumerable<ExpenditureDto> Items, int TotalCount)> GetPagedAsync(ExpenditureQueryParameters parameters)
    {
        var (items, total) = await _repository.GetPagedAsync(parameters);
        return (items.Select(MapToDto), total);
    }

    public async Task<IEnumerable<ExpenditureDto>> GetAllAsync()
    {
        var all = await _repository.GetAllAsync();
        return all.Select(MapToDto);
    }

    public async Task<ExpenditureSummaryDto> GetSummaryAsync()
    {
        var metrics = await _repository.GetSummaryMetricsAsync();
        return new ExpenditureSummaryDto
        {
            TotalExpenditure = metrics.TotalExpenditure,
            TotalPaid = metrics.TotalPaid,
            TotalPending = metrics.TotalPending,
            TotalOnHold = metrics.TotalOnHold,
            PendingReviewCount = metrics.PendingReviewCount,
            VerifiedCount = metrics.VerifiedCount,
            RejectedCount = metrics.RejectedCount,
            PaidCount = metrics.PaidCount,
            OnHoldCount = metrics.OnHoldCount,
            TotalCount = metrics.TotalCount,
            ByType = metrics.ByType
        };
    }

    public async Task<ExpenditureDto?> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<ExpenditureDto> CreateAsync(CreateExpenditureDto dto)
    {
        var id = await _repository.GenerateNextIdAsync();
        var displayDate = dto.ExpenseDate.ToString("dd MMM");

        var entity = new Expenditure
        {
            Id = id,
            Amount = dto.Amount,
            Description = dto.Description.Trim(),
            Recipient = dto.Recipient.Trim(),
            Invoice = dto.Invoice.Trim(),
            Date = displayDate,
            ExpenseDate = dto.ExpenseDate,
            Verification = "Pending Review",
            PaymentStatus = "On Hold",
            Type = dto.Type,
            Notes = dto.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<ExpenditureDto?> UpdateAsync(string id, UpdateExpenditureDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Amount = dto.Amount;
        entity.Description = dto.Description.Trim();
        entity.Recipient = dto.Recipient.Trim();
        entity.Invoice = dto.Invoice.Trim();
        entity.ExpenseDate = dto.ExpenseDate;
        entity.Date = dto.ExpenseDate.ToString("dd MMM");
        entity.Type = dto.Type;
        entity.Notes = dto.Notes.Trim();

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<ExpenditureDto?> UpdateVerificationAsync(string id, string verification)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Verification = verification;

        // Auto-adjust payment status
        if (verification == "Verified" && entity.PaymentStatus == "On Hold")
            entity.PaymentStatus = "Pending";
        else if (verification == "Rejected")
            entity.PaymentStatus = "On Hold";

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<ExpenditureDto?> UpdatePaymentStatusAsync(string id, string paymentStatus)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        if (paymentStatus == "Paid" && entity.Verification != "Verified")
            throw new InvalidOperationException("Only verified expenses can be marked as paid.");

        entity.PaymentStatus = paymentStatus;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<int> BulkActionAsync(BulkExpenditureActionDto dto)
    {
        return await _repository.BulkUpdateAsync(dto.Ids, dto.Action);
    }

    private static ExpenditureDto MapToDto(Expenditure e) => new()
    {
        Id = e.Id,
        Amount = e.Amount,
        Description = e.Description,
        Recipient = e.Recipient,
        Invoice = e.Invoice,
        Date = e.Date,
        ExpenseDate = e.ExpenseDate,
        Verification = e.Verification,
        PaymentStatus = e.PaymentStatus,
        Type = e.Type,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
