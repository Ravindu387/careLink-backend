namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IMessageService
{
    Task<ApiResponse<IEnumerable<ConversationDto>>> GetConversationsAsync(string? citizenName = null, string? search = null);
    Task<ApiResponse<ConversationDto>> GetConversationByIdAsync(string id);
    Task<ApiResponse<ConversationDto>> CreateConversationAsync(CreateConversationRequest request);
    Task<ApiResponse<ChatMessageDto>> SendMessageAsync(string conversationId, SendMessageRequest request);
    Task<ApiResponse<bool>> MarkAsReadAsync(string conversationId);
    Task<ApiResponse<bool>> UpdateStatusAsync(string conversationId, UpdateConversationStatusRequest request);
    Task<ApiResponse<ConversationSummaryDto>> GetSummaryAsync(string? citizenName = null);
    Task<ApiResponse<bool>> DeleteConversationAsync(string id);
}
