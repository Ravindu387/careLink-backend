namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryMessageRepository : IMessageRepository
{
    private readonly ConcurrentDictionary<string, MessageConversation> _conversations = new();
    private readonly Lock _lock = new();

    public InMemoryMessageRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<MessageConversation>> GetAllAsync(string? citizenName = null, string? search = null)
    {
        var query = _conversations.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            query = query.Where(c => c.CitizenName.Equals(citizenName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.TeamName.ToLowerInvariant().Contains(s) ||
                c.Subject.ToLowerInvariant().Contains(s) ||
                c.Category.ToLowerInvariant().Contains(s) ||
                c.Messages.Any(m => m.Text.ToLowerInvariant().Contains(s))
            );
        }

        var sorted = query.OrderByDescending(c => c.LastMessageAt).ToList();
        return Task.FromResult<IEnumerable<MessageConversation>>(sorted);
    }

    public Task<MessageConversation?> GetByIdAsync(string id)
    {
        _conversations.TryGetValue(id, out var conversation);
        return Task.FromResult(conversation);
    }

    public Task<MessageConversation> CreateConversationAsync(MessageConversation conversation)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(conversation.Id))
            {
                conversation.Id = $"CONV-{_conversations.Count + 1}";
            }

            if (conversation.CreatedAt == default)
            {
                conversation.CreatedAt = DateTime.UtcNow;
            }

            if (conversation.LastMessageAt == default)
            {
                conversation.LastMessageAt = DateTime.UtcNow;
            }

            _conversations[conversation.Id] = conversation;
            return Task.FromResult(conversation);
        }
    }

    public Task<ChatMessage?> AddMessageAsync(string conversationId, ChatMessage message)
    {
        lock (_lock)
        {
            if (!_conversations.TryGetValue(conversationId, out var conversation))
            {
                return Task.FromResult<ChatMessage?>(null);
            }

            if (string.IsNullOrWhiteSpace(message.Id))
            {
                message.Id = $"MSG-{DateTime.UtcNow.Ticks % 1000000}";
            }

            if (message.CreatedAt == default)
            {
                message.CreatedAt = DateTime.UtcNow;
            }

            message.ConversationId = conversationId;
            conversation.Messages.Add(message);
            conversation.LastMessageAt = message.CreatedAt;

            if (message.Sender == "support" && !message.IsRead)
            {
                conversation.UnreadCount++;
            }

            return Task.FromResult<ChatMessage?>(message);
        }
    }

    public Task<bool> MarkAsReadAsync(string conversationId)
    {
        lock (_lock)
        {
            if (!_conversations.TryGetValue(conversationId, out var conversation))
            {
                return Task.FromResult(false);
            }

            conversation.UnreadCount = 0;
            foreach (var msg in conversation.Messages)
            {
                msg.IsRead = true;
            }

            return Task.FromResult(true);
        }
    }

    public Task<bool> UpdateStatusAsync(string conversationId, string status)
    {
        lock (_lock)
        {
            if (!_conversations.TryGetValue(conversationId, out var conversation))
            {
                return Task.FromResult(false);
            }

            conversation.Status = status;
            return Task.FromResult(true);
        }
    }

    public Task<bool> DeleteConversationAsync(string id)
    {
        return Task.FromResult(_conversations.TryRemove(id, out _));
    }

    public Task<int> GetTotalUnreadCountAsync(string? citizenName = null)
    {
        var query = _conversations.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            query = query.Where(c => c.CitizenName.Equals(citizenName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        var total = query.Sum(c => c.UnreadCount);
        return Task.FromResult(total);
    }

    private void SeedInitialData()
    {
        var now = DateTime.UtcNow;

        var conv1 = new MessageConversation
        {
            Id = "1",
            TeamName = "Case Support Team",
            Subject = "Emergency shelter",
            Category = "Shelter",
            CitizenName = "Samira Khan",
            CitizenId = "CIT-001",
            RequestId = "REQ-2841",
            Status = "Active",
            UnreadCount = 0,
            CreatedAt = now.AddHours(-3),
            LastMessageAt = now.AddMinutes(-8),
            Messages = new List<ChatMessage>
            {
                new()
                {
                    Id = "MSG-101",
                    ConversationId = "1",
                    Sender = "support",
                    SenderName = "Support Officer",
                    Text = "Hello Samira. We reviewed your request and are checking an accessible shelter near your location.",
                    IsRead = true,
                    CreatedAt = now.AddMinutes(-20)
                },
                new()
                {
                    Id = "MSG-102",
                    ConversationId = "1",
                    Sender = "user",
                    SenderName = "Samira Khan",
                    Text = "Thank you. One member of our household has limited mobility. We need step-free access.",
                    IsRead = true,
                    CreatedAt = now.AddMinutes(-15)
                },
                new()
                {
                    Id = "MSG-103",
                    ConversationId = "1",
                    Sender = "support",
                    SenderName = "Support Officer",
                    Text = "Understood. We have added that requirement. Riverton High School currently has accessible space, and we are confirming transport.",
                    IsRead = true,
                    CreatedAt = now.AddMinutes(-8)
                }
            }
        };

        var conv2 = new MessageConversation
        {
            Id = "2",
            TeamName = "North Ward Delivery",
            Subject = "Food and drinking water",
            Category = "Food & Water",
            CitizenName = "Samira Khan",
            CitizenId = "CIT-001",
            RequestId = "REQ-2794",
            Status = "Active",
            UnreadCount = 1,
            CreatedAt = now.AddHours(-5),
            LastMessageAt = now.AddMinutes(-42),
            Messages = new List<ChatMessage>
            {
                new()
                {
                    Id = "MSG-201",
                    ConversationId = "2",
                    Sender = "support",
                    SenderName = "Delivery Dispatch",
                    Text = "Your food and drinking water request has been received.",
                    IsRead = true,
                    CreatedAt = now.AddHours(-1)
                },
                new()
                {
                    Id = "MSG-202",
                    ConversationId = "2",
                    Sender = "support",
                    SenderName = "Delivery Dispatch",
                    Text = "Your delivery is expected by 14:20 today. Driver dispatch tag: DSP-1092.",
                    IsRead = false,
                    CreatedAt = now.AddMinutes(-42)
                }
            }
        };

        var conv3 = new MessageConversation
        {
            Id = "3",
            TeamName = "Medical Response Team",
            Subject = "Medical support",
            Category = "Medical",
            CitizenName = "Samira Khan",
            CitizenId = "CIT-001",
            RequestId = "REQ-2650",
            Status = "Active",
            UnreadCount = 0,
            CreatedAt = now.AddDays(-2),
            LastMessageAt = now.AddDays(-1),
            Messages = new List<ChatMessage>
            {
                new()
                {
                    Id = "MSG-301",
                    ConversationId = "3",
                    Sender = "support",
                    SenderName = "Dr. Alvarez",
                    Text = "We received your medical support request. Please confirm the required medication details.",
                    IsRead = true,
                    CreatedAt = now.AddDays(-2).AddHours(2)
                },
                new()
                {
                    Id = "MSG-302",
                    ConversationId = "3",
                    Sender = "support",
                    SenderName = "Dr. Alvarez",
                    Text = "Thank you for the confirmation. Prescription bundle MED-4401 has been dispatched.",
                    IsRead = true,
                    CreatedAt = now.AddDays(-1)
                }
            }
        };

        var conv4 = new MessageConversation
        {
            Id = "4",
            TeamName = "Shelter Coordinator",
            Subject = "Bedding & supplies allocation",
            Category = "Shelter",
            CitizenName = "Samira Khan",
            CitizenId = "CIT-001",
            RequestId = "REQ-2841",
            Status = "Active",
            UnreadCount = 1,
            CreatedAt = now.AddDays(-3),
            LastMessageAt = now.AddMinutes(-12),
            Messages = new List<ChatMessage>
            {
                new()
                {
                    Id = "MSG-401",
                    ConversationId = "4",
                    Sender = "support",
                    SenderName = "Shelter Officer Marcus",
                    Text = "Blankets and thermal emergency kits have been reserved under your name at Riverton Center.",
                    IsRead = false,
                    CreatedAt = now.AddMinutes(-12)
                }
            }
        };

        _conversations[conv1.Id] = conv1;
        _conversations[conv2.Id] = conv2;
        _conversations[conv3.Id] = conv3;
        _conversations[conv4.Id] = conv4;
    }
}
