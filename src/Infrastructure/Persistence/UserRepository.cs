using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using Infrastructure.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class FollowRepository : IFollowRepository
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

public class UserRepository : IUserRepository
{
    private readonly PosterDbContext _db;
    public UserRepository(PosterDbContext db)
    {
        _db = db;
    }
    
    public Task AddAsync(User user)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }
    
    public Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        return Task.CompletedTask;
    }
    
    public async Task<IEnumerable<IError>> UserExistsAsync(User user, CancellationToken cancellationToken)
    {
        /*var errors = new List<IError>();
        if (await _db.Users.AnyAsync(u => u.Username.Value == user.Username.Value, cancellationToken: cancellationToken))
        {
            errors.Add(new Error($"Username '{user.Username.Value}' already exists."));
        }
        
        if (await _db.Users.AnyAsync(u => u.Email.Value == user.Email.Value, cancellationToken: cancellationToken))
        {
            errors.Add(new Error($"Email '{user.Email.Value}' already exists."));
        }
        return errors;*/ return null;
    }

    public async Task<UserDto?> GetUserDtoAsync(Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .Where(u => u.ID == userID)
            .Select(u => new UserDto(
                u.ID, 
                u.Username, 
                u.AvatarPath
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserProfileDto?> GetUserProfileDtoAsync(Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .Where(u => u.ID == userID)
            .Select(u => new UserProfileDto(
                u.ID,
                u.Username,
                u.AvatarPath,
                u.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserProfileDto?> GetUserProfileDtoByNameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .Where(u => u.Username == username)
            .Select(u => new UserProfileDto(
                u.ID,
                u.Username,
                u.AvatarPath,
                u.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUserFollowersAsync(Guid userID, CancellationToken ct = default)
    {
        return await _db.UserFollows
            .AsNoTracking()
            .Where(uf => uf.FollowedID == userID)
            .Include(uf => uf.Follower)
            .Select(uf => new UserDto(
                uf.Follower.ID,
                uf.Follower.Username,
                uf.Follower.AvatarPath
            ))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUserFollowingAsync(Guid userID, CancellationToken ct = default)
    {
        return await _db.UserFollows
            .AsNoTracking()
            .Where(uf => uf.FollowerID == userID)
            .Include(uf => uf.Followed)
            .Select(uf => new UserDto(
                uf.Followed.ID,
                uf.Followed.Username,
                uf.Followed.AvatarPath
            ))
            .ToListAsync(ct);
    }

    public async Task<IPagedList<UserDto>> SearchByUsernameAsync(string username, int page, int pageSize, CancellationToken ct = default)
    {
        var userResponse =  _db.Users
            .AsNoTracking()
            .Where(u => EF.Functions.ILike(u.Username, $"%{username}%"))
            .OrderByDescending(u => u.Username)
            .Select(u => new UserDto(
                u.ID, 
                u.Username, 
                u.AvatarPath
            ));
        
        
        return await PagedList<UserDto>.CreateAsync(userResponse, page, pageSize, ct);
    }
    
    public async Task<User?> GetUserAsync(Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.ID == userID, cancellationToken: cancellationToken);
    }
}