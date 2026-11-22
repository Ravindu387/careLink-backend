using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class OrganisationProfileService : IOrganisationProfileService
{
    private readonly IOrganisationProfileRepository _repository;

    public OrganisationProfileService(IOrganisationProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrganisationProfileDto> GetProfileAsync(
        string orgId = "ORG-027",
        CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetProfileAsync(orgId, cancellationToken);
        return MapToDto(profile);
    }

    public async Task<OrganisationProfileDto> UpdateProfileAsync(
        string orgId,
        UpdateOrganisationProfileDto dto,
        CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetProfileAsync(orgId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.Name))
            profile.Name = dto.Name.Trim();

        if (!string.IsNullOrWhiteSpace(dto.LegalName))
            profile.LegalName = dto.LegalName.Trim();

        if (!string.IsNullOrWhiteSpace(dto.RegistrationNumber))
            profile.RegistrationNumber = dto.RegistrationNumber.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Type))
            profile.Type = dto.Type.Trim();

        if (!string.IsNullOrWhiteSpace(dto.HeadOffice))
            profile.HeadOffice = dto.HeadOffice.Trim();

        if (!string.IsNullOrWhiteSpace(dto.PrimaryContact))
            profile.PrimaryContact = dto.PrimaryContact.Trim();

        if (!string.IsNullOrWhiteSpace(dto.ContactEmail))
            profile.ContactEmail = dto.ContactEmail.Trim();

        if (!string.IsNullOrWhiteSpace(dto.EmergencyLine))
            profile.EmergencyLine = dto.EmergencyLine.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Website))
            profile.Website = dto.Website.Trim();

        if (dto.Sectors != null && dto.Sectors.Count > 0)
            profile.Sectors = dto.Sectors.Select(s => s.Trim()).Distinct().ToList();

        profile.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateProfileAsync(profile, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<OrganisationProfileDto> UpdateCapabilitiesAsync(
        string orgId,
        UpdateCapabilitiesDto dto,
        CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetProfileAsync(orgId, cancellationToken);
        var cap = profile.Capabilities;

        if (dto.PersonnelActive.HasValue || dto.PersonnelDeployable.HasValue)
        {
            var active = dto.PersonnelActive ?? cap.Personnel.TotalCount;
            var deployable = dto.PersonnelDeployable ?? cap.Personnel.ActiveCount;
            cap.Personnel.TotalCount = active;
            cap.Personnel.ActiveCount = deployable;
            cap.Personnel.Value = $"{active} active staff";
            cap.Personnel.Subtitle = $"{deployable} deployable today";
        }

        if (dto.FleetTotal.HasValue || dto.FleetAvailable.HasValue)
        {
            var total = dto.FleetTotal ?? cap.Fleet.TotalCount;
            var avail = dto.FleetAvailable ?? cap.Fleet.ActiveCount;
            cap.Fleet.TotalCount = total;
            cap.Fleet.ActiveCount = avail;
            cap.Fleet.Value = $"{total} vehicles";
            cap.Fleet.Subtitle = $"{avail} available";
        }

        if (dto.StockCategories.HasValue || dto.StockAvailabilityPercent.HasValue)
        {
            var cats = dto.StockCategories ?? cap.Stock.TotalCount;
            var percent = dto.StockAvailabilityPercent ?? cap.Stock.ActiveCount;
            cap.Stock.TotalCount = cats;
            cap.Stock.ActiveCount = percent;
            cap.Stock.Value = $"{cats} resource categories";
            cap.Stock.Subtitle = $"{percent}% available";
        }

        if (dto.CoverageTotalDistricts.HasValue || dto.CoverageActiveDistricts.HasValue)
        {
            var dist = dto.CoverageTotalDistricts ?? cap.Coverage.TotalCount;
            var activeDist = dto.CoverageActiveDistricts ?? cap.Coverage.ActiveCount;
            cap.Coverage.TotalCount = dist;
            cap.Coverage.ActiveCount = activeDist;
            cap.Coverage.Value = $"{dist} districts";
            cap.Coverage.Subtitle = $"{activeDist} active now";
        }

        var updated = await _repository.UpdateCapabilitiesAsync(orgId, cap, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<OrganisationProfileDto> ResetDefaultProfileAsync(CancellationToken cancellationToken = default)
    {
        var reset = await _repository.ResetDefaultProfileAsync(cancellationToken);
        return MapToDto(reset);
    }

    private static OrganisationProfileDto MapToDto(OrganisationProfile entity)
    {
        return new OrganisationProfileDto
        {
            Id = entity.Id,
            Name = entity.Name,
            LegalName = entity.LegalName,
            RegistrationNumber = entity.RegistrationNumber,
            Type = entity.Type,
            RegisteredDate = entity.RegisteredDate,
            Status = entity.Status,
            Verification = entity.Verification,
            HeadOffice = entity.HeadOffice,
            PrimaryContact = entity.PrimaryContact,
            ContactEmail = entity.ContactEmail,
            EmergencyLine = entity.EmergencyLine,
            Website = entity.Website,
            ProfileCompleteness = entity.ProfileCompleteness,
            CompletenessReviewItems = entity.CompletenessReviewItems,
            Sectors = new List<string>(entity.Sectors),
            Verifications = entity.Verifications.Select(v => new OrganisationVerificationItemDto
            {
                Title = v.Title,
                Subtitle = v.Subtitle,
                Status = v.Status,
                Done = v.Done,
                DueDate = v.DueDate
            }).ToList(),
            Capabilities = new OrganisationCapabilitiesDto
            {
                Personnel = new CapabilityMetricDto
                {
                    Title = entity.Capabilities.Personnel.Title,
                    Value = entity.Capabilities.Personnel.Value,
                    Subtitle = entity.Capabilities.Personnel.Subtitle,
                    TotalCount = entity.Capabilities.Personnel.TotalCount,
                    ActiveCount = entity.Capabilities.Personnel.ActiveCount
                },
                Fleet = new CapabilityMetricDto
                {
                    Title = entity.Capabilities.Fleet.Title,
                    Value = entity.Capabilities.Fleet.Value,
                    Subtitle = entity.Capabilities.Fleet.Subtitle,
                    TotalCount = entity.Capabilities.Fleet.TotalCount,
                    ActiveCount = entity.Capabilities.Fleet.ActiveCount
                },
                Stock = new CapabilityMetricDto
                {
                    Title = entity.Capabilities.Stock.Title,
                    Value = entity.Capabilities.Stock.Value,
                    Subtitle = entity.Capabilities.Stock.Subtitle,
                    TotalCount = entity.Capabilities.Stock.TotalCount,
                    ActiveCount = entity.Capabilities.Stock.ActiveCount
                },
                Coverage = new CapabilityMetricDto
                {
                    Title = entity.Capabilities.Coverage.Title,
                    Value = entity.Capabilities.Coverage.Value,
                    Subtitle = entity.Capabilities.Coverage.Subtitle,
                    TotalCount = entity.Capabilities.Coverage.TotalCount,
                    ActiveCount = entity.Capabilities.Coverage.ActiveCount
                }
            },
            UpdatedAt = entity.UpdatedAt
        };
    }
}
