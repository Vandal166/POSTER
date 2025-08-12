using Application.DTOs;
using Domain.Entities;

namespace Application.Contracts.Persistence;

public interface IPostRepository
{
    Task<bool> ExistsAsync(Guid postId, CancellationToken cancellationToken = default);
   // Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PostDto?> GetPostAsync(Guid postID, CancellationToken ct = default);
    Task<Post?> GetPostByIDAsync(Guid postID, CancellationToken ct = default);
    
    Task<PostDto?> GetPostByCommentAsync(Guid commentID, CancellationToken cancellationToken = default);
    Task<List<Post>> GetUserFeedAsync(Guid userId, CancellationToken cancellationToken = default);

    //Keyset pagination
    Task<List<PostDto>> GetAllAsync(DateTime? lastCreatedAt, int pageSize, CancellationToken ct = default);
    
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
    Task DeleteAsync(Post post, CancellationToken cancellationToken = default);
}

public interface IConversationRepository
{
    Task<bool> ExistsAsync(Guid conversationId, CancellationToken ct = default);

    Task<ConversationDto?> GetConversationDtoAsync(Guid conversationId, Guid requestingUserID, CancellationToken ct = default);
    Task<Conversation?> GetConversationAsync(Guid conversationId, Guid requestingUserID, CancellationToken ct = default);
    Task<UserDto?> GetConversationParticipantAsync(Guid conversationId, Guid participantId, CancellationToken ct = default);

    Task<IPagedList<ConversationDto>> GetAllAsync(Guid currentUserID, int pageNumber, int pageSize, CancellationToken ct = default);
    // this is just returning the IDs of all conversations the user is part of for SignalR messages notifications
    Task<List<Guid>> GetConversationsIdsAsync(Guid requestingUserID, CancellationToken ct = default);
    
    
    Task AddAsync(Conversation conversation, CancellationToken ct = default);
    Task AddParticipantsAsync(ConversationUser conversationUser, CancellationToken ct = default);
    
    Task UpdateAsync(Conversation conversation, CancellationToken ct = default);
    
    Task DeleteAsync(Conversation conversation, CancellationToken ct = default);
    Task DeleteParticipantAsync(ConversationUser conversationUser, CancellationToken ct = default);
}

public interface IConversationMessageRepository
{
    Task<bool> ExistsAsync(Guid conversationID, Guid messageId, CancellationToken ct = default);
    
    Task<List<MessageDto>> GetMessagesByConversationAsync(Guid conversationID, Guid requestingUserID, DateTime? lastMessageAt, int pageSize, CancellationToken ct = default);
    Task<Message?> GetMessageAsync(Guid conversationID, Guid messageID, CancellationToken ct = default);
    Task<MessageDto?> GetMessageDtoAsync(Guid conversationID, Guid messageID, CancellationToken ct = default);
    
    Task AddAsync(Message message, CancellationToken ct = default);
    Task UpdateAsync(Message message, CancellationToken ct = default);
    Task DeleteAsync(Message message, CancellationToken ct = default);
}