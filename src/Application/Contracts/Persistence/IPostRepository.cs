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

    Task<ConversationDto?> GetConversationAsync(Guid conversationId, CancellationToken ct = default);
    Task<List<ConversationDto>> GetAllAsync(Guid currentUserID, DateTime? lastMessageAt, int pageSize, CancellationToken ct = default);
    
    
    Task AddAsync(Conversation conversation, CancellationToken ct = default);
    Task UpdateAsync(Conversation conversation, CancellationToken ct = default);
    Task DeleteAsync(Conversation conversation, CancellationToken ct = default);
}