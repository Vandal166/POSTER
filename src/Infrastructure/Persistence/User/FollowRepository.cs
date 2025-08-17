using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

internal sealed class FollowRepository : IFollowRepository
{
    private readonly PosterDbContext _db;
    
    public FollowRepository(PosterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsAsync(Guid followerID, Guid followingID, CancellationToken ct = default)
    {
        return await _db.UserFollows
            .AsNoTracking()
            .AnyAsync(uf => uf.FollowerID == followerID && uf.FollowedID == followingID, ct);
    }

    public async Task<UserFollow?> GetUserFollowAsync(Guid followerID, Guid followingID, CancellationToken ct = default)
    {
        return await _db.UserFollows
            .AsNoTracking()
            .FirstOrDefaultAsync(uf => uf.FollowerID == followerID && uf.FollowedID == followingID, ct);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetFollowersAsync(Guid userID, CancellationToken ct = default)
    {
        return await _db.UserFollows
            .AsNoTracking()
            .Where(uf => uf.FollowedID == userID)
            .Select(uf => new UserDto(
                uf.Follower.ID,
                uf.Follower.Username,
                uf.Follower.AvatarPath
            ))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetFollowingAsync(Guid userID, CancellationToken ct = default)
    {
        return await _db.UserFollows
            .AsNoTracking()
            .Where(uf => uf.FollowerID == userID)
            .Select(uf => new UserDto(
                uf.Followed.ID,
                uf.Followed.Username,
                uf.Followed.AvatarPath
            ))
            .ToListAsync(ct);
    }

    public Task AddAsync(UserFollow userFollow, CancellationToken ct = default)
    {
        _db.UserFollows.Add(userFollow);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserFollow userFollow, CancellationToken ct = default)
    {
        _db.UserFollows.Remove(userFollow);
        return Task.CompletedTask;
    }
}