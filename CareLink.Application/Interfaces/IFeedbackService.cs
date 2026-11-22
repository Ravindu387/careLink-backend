namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IFeedbackService
{
    Task<ApiResponse<IEnumerable<FeedbackDto>>> GetFeedbacksAsync(
        string? category,
        int? rating,
        string? status,
        string? search);
    Task<ApiResponse<FeedbackDto>> GetFeedbackByIdAsync(string id);
    Task<ApiResponse<FeedbackSummaryDto>> GetSummaryAsync();
    Task<ApiResponse<FeedbackDto>> CreateFeedbackAsync(CreateFeedbackRequest request);
    Task<ApiResponse<FeedbackDto>> UpdateStatusAsync(string id, UpdateFeedbackStatusRequest request);
    Task<ApiResponse<bool>> DeleteFeedbackAsync(string id);
}
