using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly PosterDbContext _db;
    public ConversationRepository(PosterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsAsync(Guid conversationId, CancellationToken ct = default)
    {
        return await _db.Conversations
            .AsNoTracking()
            .AnyAsync(c => c.ID == conversationId, ct);
    }

    public async Task<ConversationDto?> GetConversationDtoAsync(Guid conversationId, Guid requestingUserID, CancellationToken ct = default)
    {
        return await _db.Conversations
            .AsNoTracking()
            .Where(c => c.ID == conversationId && c.Participants.Any(p => p.UserID == requestingUserID)) // ensuring the user is part of the conversation
            .Select(c => new ConversationDto(
                c.ID,
                c.Name,
                c.ProfilePictureID,
                c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.Content).FirstOrDefault()!,
                c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.CreatedAt).FirstOrDefault(),
                c.CreatedAt,
                c.CreatedByID
            ))
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<Conversation?> GetConversationAsync(Guid conversationId, Guid requestingUserID, CancellationToken ct = default)
    {
        return await _db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.ID == conversationId && c.Participants.Any(p => p.UserID == requestingUserID), ct);
    }

   // ConversationRepository.cs
public async Task<List<ConversationDto>> GetAllAsync(
    Guid currentUserID, 
    DateTime? lastMessageAt, 
    DateTime? lastConvCreationDate, 
    int pageSize, 
    CancellationToken ct = default)
    {
        var query = _db.Conversations
            .AsNoTracking()
            .Where(c => c.Participants.Any(p => p.UserID == currentUserID))
            .Select(c => new
            {
                Conversation = c,
                LastMessageCreatedAt = (DateTime?)c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.CreatedAt)
                    .FirstOrDefault(),
                LastMessageContent = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefault()
            });

        if (lastMessageAt.HasValue && lastConvCreationDate.HasValue)
        {
            var utcLastMessageAt = lastMessageAt.Value.ToUniversalTime();
            var utcLastConvCreation = lastConvCreationDate.Value.ToUniversalTime();
            
            query = query.Where(x => 
                (x.LastMessageCreatedAt.HasValue && 
                 x.LastMessageCreatedAt < utcLastMessageAt) ||
                (x.LastMessageCreatedAt.HasValue && 
                 x.LastMessageCreatedAt == utcLastMessageAt && 
                 x.Conversation.CreatedAt < utcLastConvCreation) ||
                (!x.LastMessageCreatedAt.HasValue && 
                 x.Conversation.CreatedAt < utcLastConvCreation));
        }

        return await query
            .OrderByDescending(x => x.LastMessageCreatedAt.HasValue)
            .ThenByDescending(x => x.LastMessageCreatedAt ?? x.Conversation.CreatedAt)
            .Take(pageSize)
            .Select(x => new ConversationDto(
                x.Conversation.ID,
                x.Conversation.Name,
                x.Conversation.ProfilePictureID,
                x.LastMessageContent!,
                x.LastMessageCreatedAt.GetValueOrDefault(),
                x.Conversation.CreatedAt,
                x.Conversation.CreatedByID
            ))
            .ToListAsync(ct);
    }

    public Task AddAsync(Conversation conversation, CancellationToken ct = default)
    {
        _db.Conversations.Add(conversation);
        return Task.CompletedTask;
    }

    public Task AddParticipantsAsync(ConversationUser conversationUser, CancellationToken ct = default)
    {
        _db.ConversationUsers.Add(conversationUser);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(Conversation conversation, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Conversation conversation, CancellationToken ct = default)
    {
        _db.Conversations.Remove(conversation);
        return Task.CompletedTask;
    }
}