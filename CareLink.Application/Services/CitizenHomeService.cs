namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class CitizenHomeService : ICitizenHomeService
{
    private readonly ICitizenHomeRepository _repository;

    private static readonly List<HelpCategoryDto> DefaultCategories =
    [
        new() { Title = "Emergency shelter", Description = "Find a safe place to stay", IconKey = "House" },
        new() { Title = "Food and water", Description = "Request essential supplies", IconKey = "Utensils" },
        new() { Title = "Medical support", Description = "Get urgent health assistance", IconKey = "Cross" },
        new() { Title = "Safe transport", Description = "Ask for evacuation or accessible travel", IconKey = "Truck" },
        new() { Title = "Family support", Description = "Help for children, older adults, or disability", IconKey = "Users" },
        new() { Title = "Other essential help", Description = "Tell us what you need", IconKey = "Package" }
    ];

    public CitizenHomeService(ICitizenHomeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<CitizenHomeOverviewDto>> GetHomeOverviewAsync(string? citizenName = null)
    {
        var operation = await _repository.GetActiveOperationAsync();
        var allRequests = (await _repository.GetCitizenRequestsAsync(citizenName)).ToList();
        var facilities = (await _repository.GetNearbyFacilitiesAsync()).ToList();

        var activeRequests = allRequests
            .Where(r => !string.Equals(r.Status, "Completed", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(r.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            .Select(MapToDto)
            .ToList();

        var overview = new CitizenHomeOverviewDto
        {
            Operation = MapOperationToDto(operation),
            ActiveRequests = activeRequests,
            NearbyFacilities = facilities.Select(MapFacilityToDto).ToList(),
            Categories = DefaultCategories,
            Summary = new CitizenHomeSummaryDto
            {
                ActiveRequestsCount = activeRequests.Count,
                NearbyFacilitiesCount = facilities.Count,
                ActiveAlertsCount = string.IsNullOrWhiteSpace(operation.ActiveAlertTitle) ? 0 : 1,
                UnreadNotificationsCount = 4
            }
        };

        return ApiResponse<CitizenHomeOverviewDto>.SuccessResponse(overview, "Citizen home overview loaded successfully");
    }

    public async Task<ApiResponse<IEnumerable<CitizenRequestDto>>> GetRequestsAsync(string? citizenName = null, string? search = null, string? status = null)
    {
        var requests = await _repository.GetCitizenRequestsAsync(citizenName, search, status);
        var dtos = requests.Select(MapToDto);
        return ApiResponse<IEnumerable<CitizenRequestDto>>.SuccessResponse(dtos, "Citizen requests retrieved successfully");
    }

    public async Task<ApiResponse<CitizenRequestDto>> GetRequestByIdAsync(string id)
    {
        var request = await _repository.GetCitizenRequestByIdAsync(id);
        if (request == null)
        {
            return ApiResponse<CitizenRequestDto>.ErrorResponse($"Citizen request with id '{id}' not found");
        }

        return ApiResponse<CitizenRequestDto>.SuccessResponse(MapToDto(request), "Citizen request retrieved successfully");
    }

    public async Task<ApiResponse<CitizenRequestDto>> CreateRequestAsync(CreateCitizenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return ApiResponse<CitizenRequestDto>.ErrorResponse("Category is required");
        }

        var category = request.Category.Trim();
        var title = category;
        var description = !string.IsNullOrWhiteSpace(request.Description)
            ? request.Description.Trim()
            : $"Request for {category} submitted. A relief team member will review your details shortly.";

        var color = category.ToLowerInvariant() switch
        {
            "food and water" => "green",
            "medical support" => "green",
            "emergency shelter" => "blue",
            _ => "blue"
        };

        var entity = new CitizenRequest
        {
            Title = title,
            Category = category,
            Description = description,
            Status = "Under review",
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Normal" : request.Priority.Trim(),
            CitizenName = string.IsNullOrWhiteSpace(request.CitizenName) ? "Samira Khan" : request.CitizenName.Trim(),
            ContactPhone = string.IsNullOrWhiteSpace(request.ContactPhone) ? "+94 77 123 4567" : request.ContactPhone.Trim(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? "North Ward, Greater Riverton" : request.Address.Trim(),
            Color = color,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateCitizenRequestAsync(entity);
        return ApiResponse<CitizenRequestDto>.SuccessResponse(MapToDto(created), "Help request submitted successfully");
    }

    public async Task<ApiResponse<CitizenRequestDto>> UpdateRequestStatusAsync(string id, UpdateCitizenRequestStatus request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return ApiResponse<CitizenRequestDto>.ErrorResponse("Status cannot be empty");
        }

        var updated = await _repository.UpdateCitizenRequestStatusAsync(id, request.Status.Trim());
        if (updated == null)
        {
            return ApiResponse<CitizenRequestDto>.ErrorResponse($"Citizen request with id '{id}' not found");
        }

        return ApiResponse<CitizenRequestDto>.SuccessResponse(MapToDto(updated), "Citizen request status updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteRequestAsync(string id)
    {
        var deleted = await _repository.DeleteCitizenRequestAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Citizen request with id '{id}' not found");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Citizen request removed successfully");
    }

    public async Task<ApiResponse<CitizenRequestsSummaryDto>> GetRequestsSummaryAsync(string? citizenName = null)
    {
        var requests = (await _repository.GetCitizenRequestsAsync(citizenName)).ToList();
        var summary = new CitizenRequestsSummaryDto
        {
            TotalRequests = requests.Count,
            ActiveCount = requests.Count(r => !r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && !r.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)),
            CompletedCount = requests.Count(r => r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            DraftsCount = 0
        };
        return ApiResponse<CitizenRequestsSummaryDto>.SuccessResponse(summary, "Citizen requests summary loaded successfully");
    }

    public async Task<ApiResponse<IEnumerable<NearbyFacilityDto>>> GetFacilitiesAsync(
        string? search = null,
        string? category = null,
        string? availability = null,
        string? distance = null)
    {
        var facilities = (await _repository.GetNearbyFacilitiesAsync(search)).ToList();

        // Category filter
        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All support", StringComparison.OrdinalIgnoreCase))
        {
            facilities = facilities.Where(f =>
                (category.Equals("Shelters", StringComparison.OrdinalIgnoreCase) && f.Category.Contains("shelter", StringComparison.OrdinalIgnoreCase)) ||
                (category.Equals("Medical", StringComparison.OrdinalIgnoreCase) && (f.Category.Contains("medical", StringComparison.OrdinalIgnoreCase) || f.Category.Contains("health", StringComparison.OrdinalIgnoreCase))) ||
                (category.Equals("Support centres", StringComparison.OrdinalIgnoreCase) && f.Category.Contains("support", StringComparison.OrdinalIgnoreCase)) ||
                f.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Availability filter
        if (!string.IsNullOrWhiteSpace(availability) && !availability.Equals("All availability", StringComparison.OrdinalIgnoreCase))
        {
            if (availability.Equals("Open now", StringComparison.OrdinalIgnoreCase))
            {
                facilities = facilities.Where(f => !f.Status.Equals("Full", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else if (availability.Equals("Space available", StringComparison.OrdinalIgnoreCase))
            {
                facilities = facilities.Where(f => f.Status.Equals("Space available", StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        // Distance filter
        if (!string.IsNullOrWhiteSpace(distance) && !distance.Equals("Any distance", StringComparison.OrdinalIgnoreCase))
        {
            if (distance.Equals("Within 2 km", StringComparison.OrdinalIgnoreCase))
            {
                facilities = facilities.Where(f => f.DistanceKm <= 2.0).ToList();
            }
            else if (distance.Equals("Within 5 km", StringComparison.OrdinalIgnoreCase))
            {
                facilities = facilities.Where(f => f.DistanceKm <= 5.0).ToList();
            }
        }

        var dtos = facilities.Select(MapFacilityToDto);
        return ApiResponse<IEnumerable<NearbyFacilityDto>>.SuccessResponse(dtos, "Nearby facilities retrieved successfully");
    }

    public async Task<ApiResponse<NearbyFacilityDto>> GetFacilityByIdAsync(string id)
    {
        var facility = await _repository.GetFacilityByIdAsync(id);
        if (facility == null)
        {
            return ApiResponse<NearbyFacilityDto>.ErrorResponse($"Facility with ID '{id}' was not found.");
        }
        return ApiResponse<NearbyFacilityDto>.SuccessResponse(MapFacilityToDto(facility), "Facility retrieved successfully");
    }

    public async Task<ApiResponse<NearbySupportSummaryDto>> GetNearbySupportSummaryAsync()
    {
        var facilities = (await _repository.GetNearbyFacilitiesAsync()).ToList();
        var summary = new NearbySupportSummaryDto
        {
            TotalLocations = facilities.Count,
            OpenCount = facilities.Count(f => !f.Status.Equals("Full", StringComparison.OrdinalIgnoreCase)),
            SheltersCount = facilities.Count(f => f.Category.Contains("shelter", StringComparison.OrdinalIgnoreCase)),
            MedicalCount = facilities.Count(f => f.Category.Contains("medical", StringComparison.OrdinalIgnoreCase) || f.Category.Contains("health", StringComparison.OrdinalIgnoreCase)),
            SupportCentresCount = facilities.Count(f => f.Category.Contains("support", StringComparison.OrdinalIgnoreCase))
        };
        return ApiResponse<NearbySupportSummaryDto>.SuccessResponse(summary, "Nearby support summary loaded successfully");
    }

    public async Task<ApiResponse<NearbyFacilityDto>> UpdateFacilityStatusAsync(string id, string status)
    {
        var updated = await _repository.UpdateFacilityStatusAsync(id, status);
        if (updated == null)
        {
            return ApiResponse<NearbyFacilityDto>.ErrorResponse($"Facility with ID '{id}' was not found.");
        }
        return ApiResponse<NearbyFacilityDto>.SuccessResponse(MapFacilityToDto(updated), "Facility status updated successfully");
    }

    public async Task<ApiResponse<NearbyFacilityDto>> CreateFacilityAsync(CreateFacilityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<NearbyFacilityDto>.ErrorResponse("Facility name is required.");
        }

        var distanceKm = request.DistanceKm ?? 1.5;
        var facility = new NearbyFacility
        {
            Name = request.Name.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "Support centre" : request.Category.Trim(),
            Distance = string.IsNullOrWhiteSpace(request.Distance) ? $"{distanceKm:F1} km" : request.Distance.Trim(),
            DistanceKm = distanceKm,
            Hours = string.IsNullOrWhiteSpace(request.Hours) ? "Open 24 hours" : request.Hours.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Open" : request.Status.Trim(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? "Greater Riverton" : request.Address.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? "+94 21 222 1000" : request.Phone.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Color = string.IsNullOrWhiteSpace(request.Color) ? "green" : request.Color.Trim(),
            MapX = request.MapX ?? 50,
            MapY = request.MapY ?? 50,
            IconKey = string.IsNullOrWhiteSpace(request.IconKey) ? "House" : request.IconKey.Trim(),
            Services = request.Services ?? ["Emergency Aid"],
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateFacilityAsync(facility);
        return ApiResponse<NearbyFacilityDto>.SuccessResponse(MapFacilityToDto(created), "Facility created successfully");
    }

    public async Task<ApiResponse<CrisisOperationDto>> GetActiveOperationAsync()
    {
        var op = await _repository.GetActiveOperationAsync();
        return ApiResponse<CrisisOperationDto>.SuccessResponse(MapOperationToDto(op), "Active operation retrieved successfully");
    }

    private static CitizenRequestDto MapToDto(CitizenRequest req)
    {
        var progress = CalculateProgress(req.Status);
        var color = !string.IsNullOrWhiteSpace(req.Color) ? req.Color : CalculateColor(progress);

        return new CitizenRequestDto
        {
            Id = req.Id,
            Title = req.Title,
            Category = req.Category,
            Description = req.Description,
            Status = req.Status,
            Priority = req.Priority,
            CitizenName = req.CitizenName,
            ContactPhone = req.ContactPhone,
            Address = req.Address,
            Color = color,
            TimeAgo = FormatTimeAgo(req.CreatedAt),
            Progress = progress,
            FormattedDate = $"Submitted {req.CreatedAt.ToLocalTime():dd MMM yyyy · HH:mm}",
            Updated = FormatTimeAgo(req.UpdatedAt ?? req.CreatedAt),
            CanMessage = !req.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && !req.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase),
            DispatchTrackingId = req.Description.Contains("DSP-") ? System.Text.RegularExpressions.Regex.Match(req.Description, @"DSP-\d+").Value : null,
            CreatedAt = req.CreatedAt,
            UpdatedAt = req.UpdatedAt
        };
    }

    private static int CalculateProgress(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return 1;
        var s = status.Trim().ToLowerInvariant();
        if (s.Contains("completed") || s.Contains("delivered") || s.Contains("resolved")) return 4;
        if (s.Contains("way") || s.Contains("arranged") || s.Contains("dispatch") || s.Contains("transit")) return 3;
        if (s.Contains("review") || s.Contains("progress") || s.Contains("assigned") || s.Contains("processing")) return 2;
        return 1;
    }

    private static string CalculateColor(int progress)
    {
        return progress switch
        {
            4 => "green",
            3 => "blue",
            2 => "amber",
            _ => "blue"
        };
    }

    private static NearbyFacilityDto MapFacilityToDto(NearbyFacility f)
    {
        return new NearbyFacilityDto
        {
            Id = f.Id,
            Name = f.Name,
            Category = f.Category,
            Distance = f.Distance,
            DistanceKm = f.DistanceKm,
            Hours = f.Hours,
            Status = f.Status,
            Address = f.Address,
            Phone = f.Phone,
            Description = f.Description,
            Color = f.Color,
            MapX = f.MapX,
            MapY = f.MapY,
            IconKey = f.IconKey,
            Services = f.Services
        };
    }

    private static CrisisOperationDto MapOperationToDto(CrisisOperation op)
    {
        return new CrisisOperationDto
        {
            Id = op.Id,
            Title = op.Title,
            Subtitle = op.Subtitle,
            Status = op.Status,
            Region = op.Region,
            ActiveAlertTitle = op.ActiveAlertTitle,
            ActiveAlertDescription = op.ActiveAlertDescription,
            ActiveAlertSeverity = op.ActiveAlertSeverity,
            Helpline = op.Helpline
        };
    }

    private static string FormatTimeAgo(DateTime dateTime)
    {
        var diff = DateTime.UtcNow - dateTime;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{Math.Max(1, (int)diff.TotalMinutes)} minutes ago";
        if (diff.TotalHours < 24) return $"Today · {dateTime:HH:mm}";
        if (diff.TotalDays < 2) return $"Yesterday · {dateTime:HH:mm}";
        return $"{dateTime:dd MMM} · {dateTime:HH:mm}";
    }
}
