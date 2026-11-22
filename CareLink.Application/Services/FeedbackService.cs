namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _repository;

    public FeedbackService(IFeedbackRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<FeedbackDto>>> GetFeedbacksAsync(
        string? category,
        int? rating,
        string? status,
        string? search)
    {
        var feedbacks = await _repository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            feedbacks = feedbacks.Where(f => string.Equals(f.Category, category.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (rating.HasValue && rating.Value > 0)
        {
            feedbacks = feedbacks.Where(f => f.Rating == rating.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            feedbacks = feedbacks.Where(f => string.Equals(f.Status, status.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            feedbacks = feedbacks.Where(f =>
                (f.Id?.ToLowerInvariant().Contains(s) ?? false) ||
                (f.Category?.ToLowerInvariant().Contains(s) ?? false) ||
                (f.Comment?.ToLowerInvariant().Contains(s) ?? false) ||
                (f.SubmittedBy?.ToLowerInvariant().Contains(s) ?? false) ||
                (f.Status?.ToLowerInvariant().Contains(s) ?? false)
            );
        }

        var dtos = feedbacks.OrderByDescending(f => f.CreatedAt).Select(MapToDto);
        return ApiResponse<IEnumerable<FeedbackDto>>.SuccessResponse(dtos, "Feedbacks retrieved successfully");
    }

    public async Task<ApiResponse<FeedbackDto>> GetFeedbackByIdAsync(string id)
    {
        var feedback = await _repository.GetByIdAsync(id);
        if (feedback == null)
        {
            return ApiResponse<FeedbackDto>.ErrorResponse($"Feedback with id '{id}' not found");
        }

        return ApiResponse<FeedbackDto>.SuccessResponse(MapToDto(feedback), "Feedback retrieved successfully");
    }

    public async Task<ApiResponse<FeedbackSummaryDto>> GetSummaryAsync()
    {
        var all = (await _repository.GetAllAsync()).ToList();

        var summary = new FeedbackSummaryDto
        {
            TotalCount = all.Count,
            AverageRating = all.Count > 0 ? Math.Round(all.Average(f => f.Rating), 1) : 0,
            FiveStarCount = all.Count(f => f.Rating == 5),
            FourStarCount = all.Count(f => f.Rating == 4),
            ThreeStarCount = all.Count(f => f.Rating == 3),
            LowRatingCount = all.Count(f => f.Rating <= 2)
        };

        return ApiResponse<FeedbackSummaryDto>.SuccessResponse(summary, "Feedback summary retrieved successfully");
    }

    public async Task<ApiResponse<FeedbackDto>> CreateFeedbackAsync(CreateFeedbackRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return ApiResponse<FeedbackDto>.ErrorResponse("Rating must be between 1 and 5 stars");
        }

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return ApiResponse<FeedbackDto>.ErrorResponse("Feedback comment cannot be empty");
        }

        var category = string.IsNullOrWhiteSpace(request.Category) ? "Using the citizen portal" : request.Category.Trim();
        var submittedBy = request.Anonymous
            ? "Anonymous Citizen"
            : (!string.IsNullOrWhiteSpace(request.SubmittedBy) ? request.SubmittedBy.Trim() : "Citizen User");

        var feedback = new CitizenFeedback
        {
            Category = category,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            Anonymous = request.Anonymous,
            SubmittedBy = submittedBy,
            Status = "Submitted",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(feedback);
        return ApiResponse<FeedbackDto>.SuccessResponse(MapToDto(created), "Feedback submitted successfully");
    }

    public async Task<ApiResponse<FeedbackDto>> UpdateStatusAsync(string id, UpdateFeedbackStatusRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<FeedbackDto>.ErrorResponse($"Feedback with id '{id}' not found");
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return ApiResponse<FeedbackDto>.ErrorResponse("Status cannot be empty");
        }

        existing.Status = request.Status.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return ApiResponse<FeedbackDto>.SuccessResponse(MapToDto(updated!), "Feedback status updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteFeedbackAsync(string id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<bool>.ErrorResponse($"Feedback with id '{id}' not found");
        }

        var deleted = await _repository.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(deleted, "Feedback deleted successfully");
    }

    private static FeedbackDto MapToDto(CitizenFeedback feedback)
    {
        return new FeedbackDto
        {
            Id = feedback.Id,
            Category = feedback.Category,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            Anonymous = feedback.Anonymous,
            SubmittedBy = feedback.SubmittedBy,
            Status = feedback.Status,
            CreatedAt = feedback.CreatedAt,
            UpdatedAt = feedback.UpdatedAt
        };
    }
}
