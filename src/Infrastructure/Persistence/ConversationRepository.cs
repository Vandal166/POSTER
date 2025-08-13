using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.ViewModels;
using Domain.Entities;
using Infrastructure.Common;
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
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.ID == conversationId && c.Participants.Any(p => p.UserID == requestingUserID), ct);
    }
    
    public async Task<ConversationViewModel?> GetConversationViewModelAsync(Guid conversationId, Guid requestingUserID, CancellationToken ct = default)
    {
        var conversation = await GetConversationAsync(conversationId, requestingUserID, ct);
        if (conversation is null)
            return null;

        return new ConversationViewModel
        {
            Conversation = new ConversationDto(
                conversation.ID,
                conversation.Name,
                conversation.ProfilePictureID,
                conversation.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.Content).FirstOrDefault(),
                conversation.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.CreatedAt).FirstOrDefault(),
                conversation.CreatedAt,
                conversation.CreatedByID),
            
            Participants = conversation.Participants
                .Select(p => new UserDto(
                    p.User.ID,
                    p.User.Username,
                    p.User.AvatarPath
                )).ToList()
        };
    }

    public async Task<UserDto?> GetConversationParticipantAsync(Guid conversationId, Guid participantId, CancellationToken ct = default)
    {
        return await _db.ConversationUsers
            .AsNoTracking()
            .Where(cu => cu.ConversationID == conversationId && cu.UserID == participantId)
            .Select(cu => new UserDto(
                cu.User.ID,
                cu.User.Username,
                cu.User.AvatarPath
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IPagedList<ConversationDto>> GetAllAsync(Guid currentUserID, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var response =  _db.Conversations
            .Where(c => c.Participants.Any(p => p.UserID == currentUserID)) // only conversations the user is part of
            .OrderByDescending(x => x.Messages.Any()) // conversations with messages first
            .ThenByDescending(x => x.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.CreatedAt).FirstOrDefault())
            .Select(c => new ConversationDto(
                c.ID,
                c.Name,
                c.ProfilePictureID,
                c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.Content).FirstOrDefault()!,
                c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.CreatedAt).FirstOrDefault(),
                c.CreatedAt,
                c.CreatedByID
            ));
        
        return await PagedList<ConversationDto>.CreateAsync(response, pageNumber, pageSize, ct);
    }

    public async Task<List<Guid>> GetConversationsIdsAsync(Guid requestingUserID, CancellationToken ct = default)
    {
        return await _db.Conversations
            .Where(c => c.Participants.Any(p => p.UserID == requestingUserID))
            .Select(c => c.ID)
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

    public Task UpdateAsync(Conversation conversation, CancellationToken ct = default)
    {
        _db.Conversations.Update(conversation);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Conversation conversation, CancellationToken ct = default)
    {
        _db.Conversations.Remove(conversation);
        return Task.CompletedTask;
    }

    public Task DeleteParticipantAsync(ConversationUser conversationUser, CancellationToken ct = default)
    {
        _db.ConversationUsers.Remove(conversationUser);
        return Task.CompletedTask;
    }
}