using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

internal sealed class ConversationMessageRepository : IConversationMessageRepository
{
    private readonly PosterDbContext _db;
    public ConversationMessageRepository(PosterDbContext db)
    {
        _db = db;
    }


    public async Task<bool> ExistsAsync(Guid conversationID, Guid messageId, CancellationToken ct = default)
    {
        return await _db.Messages
            .AsNoTracking()
            .AnyAsync(m => m.ConversationID == conversationID && m.ID == messageId, ct);
    }

    public async Task<List<MessageDto>> GetMessagesByConversationAsync(Guid conversationId, Guid requestingUserID, DateTime? lastMessageAt, int pageSize,
        CancellationToken ct = default)
    {
        IQueryable<Message> query = _db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationID == conversationId && m.Conversation.Participants.Any(p => p.UserID == requestingUserID))
            .OrderByDescending(m => m.CreatedAt);

        if (lastMessageAt.HasValue)
        {
            var utcLastMessageAt = DateTime.SpecifyKind(lastMessageAt.Value, DateTimeKind.Utc);
            query = query.Where(m => m.CreatedAt < utcLastMessageAt);
        }

        return await query
            .Take(pageSize)
            .Select(m => new MessageDto
                (
                    m.ID,
                    m.ConversationID,
                    m.SenderID,
                    m.Sender.Username,
                    m.Sender.AvatarPath,
                    m.Content,
                    m.CreatedAt,
                    m.IsSystemMessage
                )
            )
            .ToListAsync(ct);
    }

    public async Task<Message?> GetMessageAsync(Guid conversationID, Guid messageID, CancellationToken ct = default)
    {
        return await _db.Messages
            .FirstOrDefaultAsync(m => m.ConversationID == conversationID && m.ID == messageID, ct);
    }
    
    public async Task<MessageDto?> GetMessageDtoAsync(Guid conversationID, Guid messageID, CancellationToken ct = default)
    {
        return await _db.Messages
            .Where(m => m.ConversationID == conversationID && m.ID == messageID)
            .Select(m => new MessageDto
                (
                    m.ID,
                    m.ConversationID,
                    m.SenderID,
                    m.Sender.Username,
                    m.Sender.AvatarPath,
                    m.Content,
                    m.CreatedAt,
                    m.IsSystemMessage
                )
            )
            .FirstOrDefaultAsync(ct);
    }

    public Task AddAsync(Message message, CancellationToken ct = default)
    {
        _db.Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Message message, CancellationToken ct = default)
    {
        _db.Messages.Remove(message);
        return Task.CompletedTask;
    }
}