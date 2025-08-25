using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;

namespace Infrastructure.Persistence;

internal sealed class FollowService : IFollowService
{
    private readonly IUserRepository _userRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUnitOfWork _uow;

    public FollowService(IUserRepository userRepository, IFollowRepository followRepository, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _followRepository = followRepository;
        _uow = uow;
    }

    public async Task<bool> FollowUserAsync(Guid followerID, Guid followingID, CancellationToken ct = default)
    {
        var follower = await _userRepository.GetUserAsync(followerID, ct);
        if (follower is null)
            return false;

        var following = await _userRepository.GetUserAsync(followingID, ct);
        if (following is null)
            return false;

        if(await _followRepository.ExistsAsync(followerID, followingID, ct))
            return false; // already following

        var followerUser = new UserFollow
        {
            Follower = follower,
            Followed = following,
            FollowedAt = DateTime.UtcNow
        };
        
        await _followRepository.AddAsync(followerUser, ct);
        await _uow.SaveChangesAsync(ct);
        
        return true;
    }

    public async Task<bool> UnfollowUserAsync(Guid followerID, Guid followingID, CancellationToken ct = default)
    {
        var follower = await _userRepository.GetUserAsync(followerID, ct);
        if (follower is null)
            return false;

        var following = await _userRepository.GetUserAsync(followingID, ct);
        if (following is null)
            return false;

        if (!await _followRepository.ExistsAsync(followerID, followingID, ct))
            return false; // not following
        
        var userFollow = await _followRepository.GetUserFollowAsync(followerID, followingID, ct);
        if (userFollow is null)
            return false; // not found

        await _followRepository.DeleteAsync(userFollow, ct);
        await _uow.SaveChangesAsync(ct);
        
        return true;
    }

    public async Task<bool> IsFollowingAsync(Guid followerID, Guid followingID, CancellationToken ct = default)
    {
        var following = await _userRepository.GetUserFollowingAsync(followerID, ct);
       
        return following.Any(f => f.Id == followingID);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetFollowersAsync(Guid userID, CancellationToken ct = default)
    {
        return await _followRepository.GetFollowersAsync(userID, ct);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetFollowingAsync(Guid userID, CancellationToken ct = default)
    {
        return await _followRepository.GetFollowingAsync(userID, ct);
    }

    public async Task<IReadOnlyCollection<Guid>> GetFollowingIDsAsync(Guid userID, CancellationToken ct = default)
    {
        return await _followRepository.GetFollowingIDsAsync(userID, ct);
    }
}