using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly PosterDbContext _db;
    private readonly IUnitOfWork _uow;
    public NotificationRepository(PosterDbContext db, IUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    public async Task<Notification?> GetNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.ID == notificationId, cancellationToken);
    }

    public async Task<List<NotificationDto>> GetNotificationsDtoAsync(Guid userId, DateTime? lastCreatedAt, int pageSize, CancellationToken cancellationToken = default)
    {
        IQueryable<Notification> query = _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserID == userId)
            .OrderByDescending(n => n.CreatedAt);

        if (lastCreatedAt.HasValue)
        {
            var utcCreatedAt = DateTime.SpecifyKind(lastCreatedAt.Value, DateTimeKind.Utc);
            query = query.Where(n => n.CreatedAt < utcCreatedAt);
        }

        return await query
            .Take(pageSize)
            .Select(n => new NotificationDto
                (
                    n.ID,
                    n.Message,
                    n.CreatedAt,
                    n.IsRead,
                    n.WithRedirectUrl
                )
            ).ToListAsync(cancellationToken);
    }

    public async Task<int> GetUserNotificationCount(Guid userID, CancellationToken ct = default)
    {
        return await _db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserID == userID && !n.IsRead, ct);
    }

    public async Task<List<Guid>> GetUserFollowIds(Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.UserFollows
            .AsNoTracking()
            .Where(f => f.FollowerID == userID)
            .Select(f => f.FollowedID)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetUserFollowersIds(Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.UserFollows
            .AsNoTracking()
            .Where(f => f.FollowedID == userID)
            .Select(f => f.FollowerID)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAndSaveAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _db.Notifications.Add(notification);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAndSaveAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        await _db.Notifications.AddRangeAsync(notifications, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAndSaveAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _db.Notifications.Remove(notification);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}