using System.Globalization;
using System.Text.RegularExpressions;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class DonationService : IDonationService
{
    private readonly IDonationRepository _donationRepository;

    public DonationService(IDonationRepository donationRepository)
    {
        _donationRepository = donationRepository;
    }

    public async Task<IEnumerable<DonationDto>> GetAllDonationsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _donationRepository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<PagedResultDto<DonationDto>> GetPagedDonationsAsync(DonationQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _donationRepository.GetPagedAsync(parameters, cancellationToken);

        var page = parameters.Page.GetValueOrDefault(1);
        if (page < 1) page = 1;
        var pageSize = parameters.PageSize.GetValueOrDefault(10);
        if (pageSize < 1) pageSize = 10;

        return new PagedResultDto<DonationDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DonationDto?> GetDonationByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _donationRepository.GetByIdAsync(id, cancellationToken);
        if (item == null) return null;
        return MapToDto(item);
    }

    public async Task<DonationDto> CreateDonationAsync(CreateDonationDto dto, CancellationToken cancellationToken = default)
    {
        var all = await _donationRepository.GetAllAsync(cancellationToken);
        var existingIds = all.Select(d => d.Id.ToUpperInvariant()).ToHashSet();

        // Generate ID like DON-88422
        var random = new Random();
        string id;
        do
        {
            id = $"DON-{random.Next(80000, 99999)}";
        } while (existingIds.Contains(id));

        var (receivedText, receivedDate) = ParseReceivedDates(dto.Received);
        var (formattedAmount, numericAmount) = ParseAmount(dto.Amount, dto.Type);

        var donation = new DonationRecord
        {
            Id = id,
            Donor = dto.Donor.Trim(),
            Type = dto.Type.Trim(),
            Amount = formattedAmount,
            NumericAmount = numericAmount,
            Campaign = dto.Campaign.Trim(),
            Received = receivedText,
            ReceivedDate = receivedDate,
            Verification = string.IsNullOrWhiteSpace(dto.Verification) ? "Pending" : dto.Verification.Trim(),
            Allocation = string.IsNullOrWhiteSpace(dto.Allocation) ? "Unallocated" : dto.Allocation.Trim(),
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _donationRepository.AddAsync(donation, cancellationToken);
        return MapToDto(created);
    }

    public async Task<DonationDto?> UpdateDonationAsync(string id, UpdateDonationDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _donationRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        var (receivedText, receivedDate) = ParseReceivedDates(dto.Received);
        var (formattedAmount, numericAmount) = ParseAmount(dto.Amount, dto.Type);

        existing.Donor = dto.Donor.Trim();
        existing.Type = dto.Type.Trim();
        existing.Amount = formattedAmount;
        existing.NumericAmount = numericAmount;
        existing.Campaign = dto.Campaign.Trim();
        existing.Received = receivedText;
        existing.ReceivedDate = receivedDate;
        if (!string.IsNullOrWhiteSpace(dto.Verification))
        {
            existing.Verification = dto.Verification.Trim();
        }
        if (!string.IsNullOrWhiteSpace(dto.Allocation))
        {
            existing.Allocation = dto.Allocation.Trim();
        }
        existing.Notes = dto.Notes?.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _donationRepository.UpdateAsync(existing, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<DonationDto?> UpdateDonationVerificationAsync(string id, string verification, CancellationToken cancellationToken = default)
    {
        var existing = await _donationRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Verification = verification.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _donationRepository.UpdateAsync(existing, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<DonationDto?> UpdateDonationAllocationAsync(string id, string allocation, CancellationToken cancellationToken = default)
    {
        var existing = await _donationRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Allocation = allocation.Trim();
        if (allocation.Equals("Allocated", StringComparison.OrdinalIgnoreCase))
        {
            existing.Verification = "Verified";
        }
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _donationRepository.UpdateAsync(existing, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<int> BulkActionAsync(BulkDonationActionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Ids == null || !dto.Ids.Any()) return 0;

        string? newVerification = null;
        string? newAllocation = null;

        var action = dto.Action.Trim().ToLower();
        if (action == "verify")
        {
            newVerification = "Verified";
        }
        else if (action == "allocate")
        {
            newAllocation = "Allocated";
            newVerification = "Verified";
        }
        else if (action == "flag")
        {
            newVerification = "Flagged";
        }
        else if (action == "hold")
        {
            newAllocation = "On Hold";
        }
        else
        {
            return 0;
        }

        return await _donationRepository.BulkUpdateAsync(dto.Ids, newVerification, newAllocation, cancellationToken);
    }

    public async Task<bool> DeleteDonationAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _donationRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<DonationSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _donationRepository.GetSummaryMetricsAsync(cancellationToken);
        return new DonationSummaryDto
        {
            TotalRecords = metrics.TotalRecords,
            PendingVerificationCount = metrics.PendingVerificationCount,
            MonetaryCount = metrics.MonetaryCount,
            MaterialCount = metrics.MaterialCount,
            FlaggedCount = metrics.FlaggedCount,
            TotalMonetaryValue = metrics.TotalMonetaryValue,
            AllocatedCount = metrics.AllocatedCount
        };
    }

    private static (string Display, DateTime Date) ParseReceivedDates(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            var now = DateTime.UtcNow.Date;
            return (now.ToString("dd MMM", CultureInfo.InvariantCulture), now);
        }

        if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return (parsedDate.ToString("dd MMM", CultureInfo.InvariantCulture), parsedDate.Date);
        }

        // If format like "22 Jun", attempt parse with current year
        if (DateTime.TryParseExact(input, "dd MMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedShort))
        {
            var withYear = new DateTime(DateTime.UtcNow.Year, parsedShort.Month, parsedShort.Day);
            return (input, withYear);
        }

        return (input, DateTime.UtcNow.Date);
    }

    private static (string FormattedAmount, decimal? NumericAmount) ParseAmount(string input, string type)
    {
        var clean = input?.Trim() ?? string.Empty;
        if (type.Equals("Monetary", StringComparison.OrdinalIgnoreCase))
        {
            // Extract numeric value
            var digitsOnly = Regex.Replace(clean, @"[^\d.]", "");
            if (decimal.TryParse(digitsOnly, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
            {
                var formatted = clean.StartsWith("$") ? clean : $"${num:N0}";
                return (formatted, num);
            }
            return (clean, null);
        }

        return (clean, null);
    }

    private static DonationDto MapToDto(DonationRecord entity)
    {
        return new DonationDto
        {
            Id = entity.Id,
            Donor = entity.Donor,
            Type = entity.Type,
            Amount = entity.Amount,
            NumericAmount = entity.NumericAmount,
            Campaign = entity.Campaign,
            Received = entity.Received,
            ReceivedDate = entity.ReceivedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Verification = entity.Verification,
            Allocation = entity.Allocation,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt.ToString("o"),
            UpdatedAt = entity.UpdatedAt?.ToString("o")
        };
    }
}
