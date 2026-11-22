using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class FundAllocationService : IFundAllocationService
{
    private readonly IFundAllocationRepository _repository;

    public FundAllocationService(IFundAllocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IEnumerable<FundAllocationDto> Items, int TotalCount)> GetPagedAsync(FundAllocationQueryParameters parameters)
    {
        var (items, total) = await _repository.GetPagedAsync(parameters);
        return (items.Select(MapToDto), total);
    }

    public async Task<IEnumerable<FundAllocationDto>> GetAllAsync()
    {
        var all = await _repository.GetAllAsync();
        return all.Select(MapToDto);
    }

    public async Task<FundAllocationSummaryDto> GetSummaryAsync()
    {
        var metrics = await _repository.GetSummaryMetricsAsync();
        return new FundAllocationSummaryDto
        {
            TotalAllocated = metrics.TotalAllocated,
            TotalDisbursed = metrics.TotalDisbursed,
            TotalPending = metrics.TotalPending,
            ApprovedCount = metrics.ApprovedCount,
            UnderReviewCount = metrics.UnderReviewCount,
            RejectedCount = metrics.RejectedCount,
            DisbursedCount = metrics.DisbursedCount,
            TotalCount = metrics.TotalCount,
            BySource = metrics.BySource
        };
    }

    public async Task<FundAllocationDto?> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<FundAllocationDto> CreateAsync(CreateFundAllocationDto dto)
    {
        var id = await _repository.GenerateNextIdAsync();
        var displayDate = dto.AllocationDate.ToString("dd MMM");

        var entity = new FundAllocation
        {
            Id = id,
            Amount = dto.Amount,
            Source = dto.Source.Trim(),
            Purpose = dto.Purpose.Trim(),
            Recipient = dto.Recipient.Trim(),
            Date = displayDate,
            AllocationDate = dto.AllocationDate,
            Approval = "Under Review",
            Disbursement = "Pending",
            Notes = dto.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<FundAllocationDto?> UpdateAsync(string id, UpdateFundAllocationDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Amount = dto.Amount;
        entity.Source = dto.Source.Trim();
        entity.Purpose = dto.Purpose.Trim();
        entity.Recipient = dto.Recipient.Trim();
        entity.AllocationDate = dto.AllocationDate;
        entity.Date = dto.AllocationDate.ToString("dd MMM");
        if (!string.IsNullOrWhiteSpace(dto.Approval)) entity.Approval = dto.Approval;
        if (!string.IsNullOrWhiteSpace(dto.Disbursement)) entity.Disbursement = dto.Disbursement;
        entity.Notes = dto.Notes.Trim();

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<FundAllocationDto?> UpdateApprovalAsync(string id, string approval)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Approval = approval;
        if (approval == "Rejected")
        {
            entity.Disbursement = "Not Disbursed";
        }

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<FundAllocationDto?> UpdateDisbursementAsync(string id, string disbursement)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        if (disbursement == "Disbursed" && entity.Approval != "Approved")
        {
            throw new InvalidOperationException("Only approved allocations can be disbursed.");
        }

        entity.Disbursement = disbursement;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<int> BulkActionAsync(BulkFundAllocationActionDto dto)
    {
        var approvalTarget = dto.Action switch
        {
            "Approve" => "Approved",
            "Reject" => "Rejected",
            _ => dto.Action
        };

        return await _repository.BulkUpdateApprovalAsync(dto.Ids, approvalTarget);
    }

    private static FundAllocationDto MapToDto(FundAllocation a) => new()
    {
        Id = a.Id,
        Amount = a.Amount,
        Source = a.Source,
        Purpose = a.Purpose,
        Recipient = a.Recipient,
        Date = a.Date,
        AllocationDate = a.AllocationDate,
        Approval = a.Approval,
        Disbursement = a.Disbursement,
        Notes = a.Notes,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
