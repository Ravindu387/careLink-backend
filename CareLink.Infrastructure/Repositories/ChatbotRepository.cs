using System.Collections.Concurrent;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareLink.Infrastructure.Repositories;

public class ChatbotRepository : IChatbotRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChatbotRepository> _logger;

    // Resilient memory store fallback
    private static readonly ConcurrentDictionary<string, ChatbotConversation> MemoryStore = new();

    public ChatbotRepository(ApplicationDbContext context, ILogger<ChatbotRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ChatbotConversation?> GetConversationAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_context.Database.CanConnect())
            {
                var dbConv = await _context.ChatbotConversations
                    .AsNoTracking()
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (dbConv != null)
                {
                    return dbConv;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database query failed for ChatbotConversation {Id}; checking memory store.", id);
        }

        if (MemoryStore.TryGetValue(id, out var memConv))
        {
            return memConv;
        }

        return null;
    }

    public async Task<IReadOnlyList<ChatbotConversation>> GetUserConversationsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_context.Database.CanConnect())
            {
                var dbList = await _context.ChatbotConversations
                    .AsNoTracking()
                    .Include(c => c.Messages)
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync(cancellationToken);

                if (dbList.Count > 0)
                {
                    return dbList;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database query failed for user {UserId} conversations; checking memory store.", userId);
        }

        var memoryList = MemoryStore.Values
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToList();

        return memoryList;
    }

    public async Task SaveConversationAsync(ChatbotConversation conversation, CancellationToken cancellationToken = default)
    {
        MemoryStore[conversation.Id] = conversation;

        try
        {
            if (_context.Database.CanConnect())
            {
                var existing = await _context.ChatbotConversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == conversation.Id, cancellationToken);

                if (existing == null)
                {
                    await _context.ChatbotConversations.AddAsync(conversation, cancellationToken);
                }
                else
                {
                    existing.Title = conversation.Title;
                    existing.UpdatedAt = conversation.UpdatedAt;

                    var existingIds = existing.Messages.Select(m => m.Id).ToHashSet();
                    foreach (var msg in conversation.Messages)
                    {
                        if (!existingIds.Contains(msg.Id))
                        {
                            existing.Messages.Add(msg);
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist ChatbotConversation {Id} to database; preserved in memory store.", conversation.Id);
        }
    }

    public async Task<bool> DeleteConversationAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var deletedFromDb = false;
        try
        {
            if (_context.Database.CanConnect())
            {
                var existing = await _context.ChatbotConversations
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

                if (existing != null)
                {
                    _context.ChatbotConversations.Remove(existing);
                    await _context.SaveChangesAsync(cancellationToken);
                    deletedFromDb = true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete ChatbotConversation {Id} from database; checking memory store.", id);
        }

        var deletedFromMemory = false;
        if (MemoryStore.TryGetValue(id, out var memConv) && string.Equals(memConv.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            deletedFromMemory = MemoryStore.TryRemove(id, out _);
        }

        return deletedFromDb || deletedFromMemory;
    }
}
