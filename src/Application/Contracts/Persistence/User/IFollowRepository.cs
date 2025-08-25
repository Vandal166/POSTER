using Application.DTOs;
using Domain.Entities;

namespace Application.Contracts.Persistence;

public interface IFollowRepository
{
    Task<bool> ExistsAsync(Guid followerID, Guid followingID, CancellationToken ct = default);
    
    Task<UserFollow?> GetUserFollowAsync(Guid followerID, Guid followingID, CancellationToken ct = default);
    
    Task<IReadOnlyCollection<UserDto>> GetFollowersAsync(Guid userID, CancellationToken ct = default);
    
    Task<IReadOnlyCollection<UserDto>> GetFollowingAsync(Guid userID, CancellationToken ct = default);
    
    Task<IReadOnlyCollection<Guid>> GetFollowingIDsAsync(Guid userID, CancellationToken ct = default);
    
    Task AddAsync(UserFollow userFollow, CancellationToken ct = default);
    
    Task DeleteAsync(UserFollow userFollow, CancellationToken ct = default);
}