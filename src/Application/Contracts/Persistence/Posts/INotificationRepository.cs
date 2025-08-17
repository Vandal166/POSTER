using Application.DTOs;
using Domain.Entities;

namespace Application.Contracts.Persistence;

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