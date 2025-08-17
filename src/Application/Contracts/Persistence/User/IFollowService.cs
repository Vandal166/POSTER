using Application.DTOs;

namespace Application.Contracts.Persistence;

public interface IFollowService
{
    Task<bool> FollowUserAsync(Guid followerID, Guid followingID, CancellationToken ct = default);
    
    Task<bool> UnfollowUserAsync(Guid followerID, Guid followingID, CancellationToken ct = default);
    
    Task<bool> IsFollowingAsync(Guid followerID, Guid followingID, CancellationToken ct = default);
    
    Task<IReadOnlyCollection<UserDto>> GetFollowersAsync(Guid userID, CancellationToken ct = default);
    
    Task<IReadOnlyCollection<UserDto>> GetFollowingAsync(Guid userID, CancellationToken ct = default);
}