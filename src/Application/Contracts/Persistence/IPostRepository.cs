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
    Task<List<PostDto>> GetUserFeedAsync(Guid userId, DateTime? lastCreatedAt, int pageSize, CancellationToken ct = default);

    //Keyset pagination
    Task<List<PostDto>> GetAllAsync(DateTime? lastCreatedAt, int pageSize, CancellationToken ct = default);
    
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
    Task DeleteAsync(Post post, CancellationToken cancellationToken = default);
}

public interface INotificationRepository
{
    Task<Notification?> GetNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetNotificationsDtoAsync(Guid userId, DateTime? lastCreatedAt, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUserNotificationCount(Guid userID, CancellationToken ct = default);
    
    // gets the IDs of users that the given user follows
    Task<List<Guid>> GetUserFollowIds(Guid userID, CancellationToken cancellationToken = default);
    
    // gets the IDs of users that follow the given user
    Task<List<Guid>> GetUserFollowersIds(Guid userID, CancellationToken cancellationToken = default);
    
    Task AddAndSaveAsync(Notification notification, CancellationToken cancellationToken = default);
    Task AddRangeAndSaveAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task DeleteAndSaveAsync(Notification notification, CancellationToken cancellationToken = default);
}