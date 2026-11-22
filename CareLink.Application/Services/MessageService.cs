namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _repository;

    public MessageService(IMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<ConversationDto>>> GetConversationsAsync(
        string? citizenName = null,
        string? search = null)
    {
        var conversations = await _repository.GetAllAsync(citizenName, search);
        var dtos = conversations.Select(MapToDto).ToList();
        return ApiResponse<IEnumerable<ConversationDto>>.SuccessResponse(dtos, "Conversations retrieved successfully");
    }

    public async Task<ApiResponse<ConversationDto>> GetConversationByIdAsync(string id)
    {
        var conversation = await _repository.GetByIdAsync(id);
        if (conversation == null)
        {
            return ApiResponse<ConversationDto>.ErrorResponse($"Conversation with ID '{id}' was not found.");
        }

        return ApiResponse<ConversationDto>.SuccessResponse(MapToDto(conversation), "Conversation retrieved successfully");
    }

    public async Task<ApiResponse<ConversationDto>> CreateConversationAsync(CreateConversationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<ConversationDto>.ErrorResponse("Support team name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            return ApiResponse<ConversationDto>.ErrorResponse("Conversation subject is required.");
        }

        var now = DateTime.UtcNow;
        var conversation = new MessageConversation
        {
            TeamName = request.Name.Trim(),
            Subject = request.Subject.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            CitizenName = string.IsNullOrWhiteSpace(request.CitizenName) ? "Samira Khan" : request.CitizenName.Trim(),
            RequestId = request.RequestId?.Trim(),
            Status = "Active",
            UnreadCount = 0,
            CreatedAt = now,
            LastMessageAt = now,
            Messages = new List<ChatMessage>()
        };

        if (!string.IsNullOrWhiteSpace(request.InitialMessage))
        {
            conversation.Messages.Add(new ChatMessage
            {
                Id = $"MSG-{DateTime.UtcNow.Ticks % 1000000}",
                Sender = "user",
                SenderName = conversation.CitizenName,
                Text = request.InitialMessage.Trim(),
                IsRead = true,
                CreatedAt = now
            });
        }

        var created = await _repository.CreateConversationAsync(conversation);
        return ApiResponse<ConversationDto>.SuccessResponse(MapToDto(created), "Conversation created successfully");
    }

    public async Task<ApiResponse<ChatMessageDto>> SendMessageAsync(string conversationId, SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return ApiResponse<ChatMessageDto>.ErrorResponse("Message text cannot be empty.");
        }

        var conversation = await _repository.GetByIdAsync(conversationId);
        if (conversation == null)
        {
            return ApiResponse<ChatMessageDto>.ErrorResponse($"Conversation '{conversationId}' not found.");
        }

        var sender = string.IsNullOrWhiteSpace(request.Sender) ? "user" : request.Sender.Trim().ToLowerInvariant();
        var senderName = !string.IsNullOrWhiteSpace(request.SenderName)
            ? request.SenderName.Trim()
            : (sender == "user" ? conversation.CitizenName : conversation.TeamName);

        var userMessage = new ChatMessage
        {
            Sender = sender,
            SenderName = senderName,
            Text = request.Text.Trim(),
            AttachmentUrl = request.AttachmentUrl,
            AttachmentName = request.AttachmentName,
            IsRead = sender == "user",
            CreatedAt = DateTime.UtcNow
        };

        var added = await _repository.AddMessageAsync(conversationId, userMessage);
        if (added == null)
        {
            return ApiResponse<ChatMessageDto>.ErrorResponse("Failed to save message.");
        }

        // Optional automated support acknowledgement when user sends a message
        if (sender == "user" && request.AutoReply)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1200);
                    var replyText = GenerateSupportReply(request.Text, conversation.Subject, conversation.TeamName);
                    var supportMsg = new ChatMessage
                    {
                        Sender = "support",
                        SenderName = conversation.TeamName,
                        Text = replyText,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _repository.AddMessageAsync(conversationId, supportMsg);
                }
                catch
                {
                    // Ignore background task exceptions
                }
            });
        }

        return ApiResponse<ChatMessageDto>.SuccessResponse(MapToDto(added), "Message sent successfully");
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(string conversationId)
    {
        var success = await _repository.MarkAsReadAsync(conversationId);
        if (!success)
        {
            return ApiResponse<bool>.ErrorResponse($"Conversation '{conversationId}' not found.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Conversation marked as read");
    }

    public async Task<ApiResponse<bool>> UpdateStatusAsync(string conversationId, UpdateConversationStatusRequest request)
    {
        var success = await _repository.UpdateStatusAsync(conversationId, request.Status);
        if (!success)
        {
            return ApiResponse<bool>.ErrorResponse($"Conversation '{conversationId}' not found.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Status updated successfully");
    }

    public async Task<ApiResponse<ConversationSummaryDto>> GetSummaryAsync(string? citizenName = null)
    {
        var conversations = (await _repository.GetAllAsync(citizenName)).ToList();
        var totalUnread = conversations.Sum(c => c.UnreadCount);
        var activeCount = conversations.Count(c => c.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));

        var summary = new ConversationSummaryDto
        {
            TotalConversations = conversations.Count,
            TotalUnread = totalUnread,
            ActiveConversations = activeCount
        };

        return ApiResponse<ConversationSummaryDto>.SuccessResponse(summary, "Message summary retrieved successfully");
    }

    public async Task<ApiResponse<bool>> DeleteConversationAsync(string id)
    {
        var deleted = await _repository.DeleteConversationAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Conversation '{id}' not found.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Conversation deleted successfully");
    }

    private static string GenerateSupportReply(string userText, string subject, string teamName)
    {
        var lower = userText.ToLowerInvariant();
        if (lower.Contains("thank"))
        {
            return $"You are very welcome! The {teamName} is always on standby to assist. Let us know if there is anything else you need.";
        }
        if (lower.Contains("status") || lower.Contains("when") || lower.Contains("time") || lower.Contains("eta"))
        {
            return $"We have logged your query regarding '{subject}'. Our field coordination dispatch is checking current route conditions and will follow up shortly.";
        }
        if (lower.Contains("urgent") || lower.Contains("emergency") || lower.Contains("help"))
        {
            return $"Your urgent notification has been prioritized and flagged to the on-duty supervisor. For life-threatening emergencies, remember you can also call 112 immediately.";
        }
        if (lower.Contains("mobility") || lower.Contains("access") || lower.Contains("wheelchair") || lower.Contains("transport"))
        {
            return "Requirement noted: Special accessibility accommodations have been attached to your relief profile and dispatch instructions.";
        }

        return $"Thank you for your message. {teamName} has received your update and is reviewing your file.";
    }

    private static ConversationDto MapToDto(MessageConversation entity)
    {
        var lastMsg = entity.Messages.LastOrDefault();
        var preview = lastMsg?.Text ?? "No messages yet.";

        return new ConversationDto
        {
            Id = entity.Id,
            Name = entity.TeamName,
            Subject = entity.Subject,
            Preview = preview,
            Time = FormatRelativeTime(entity.LastMessageAt),
            Unread = entity.UnreadCount,
            Category = entity.Category,
            CitizenName = entity.CitizenName,
            RequestId = entity.RequestId,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            LastMessageAt = entity.LastMessageAt,
            Messages = entity.Messages.Select(MapToDto).ToList()
        };
    }

    private static ChatMessageDto MapToDto(ChatMessage entity)
    {
        return new ChatMessageDto
        {
            Id = entity.Id,
            ConversationId = entity.ConversationId,
            Text = entity.Text,
            Sender = entity.Sender,
            SenderName = entity.SenderName,
            Time = entity.CreatedAt.ToLocalTime().ToString("HH:mm"),
            IsRead = entity.IsRead,
            AttachmentUrl = entity.AttachmentUrl,
            AttachmentName = entity.AttachmentName,
            CreatedAt = entity.CreatedAt
        };
    }

    private static string FormatRelativeTime(DateTime dateTimeUtc)
    {
        var diff = DateTime.UtcNow - dateTimeUtc;
        if (diff.TotalMinutes < 1)
        {
            return "Just now";
        }
        if (diff.TotalMinutes < 60)
        {
            return $"{(int)diff.TotalMinutes} min";
        }
        if (diff.TotalHours < 24 && dateTimeUtc.Date == DateTime.UtcNow.Date)
        {
            return dateTimeUtc.ToLocalTime().ToString("HH:mm");
        }
        if (diff.TotalDays < 2)
        {
            return "Yesterday";
        }
        return dateTimeUtc.ToLocalTime().ToString("dd MMM");
    }
}
