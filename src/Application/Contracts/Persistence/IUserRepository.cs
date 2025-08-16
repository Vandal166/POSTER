using Application.DTOs;
using Domain.Entities;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IUserRepository
{
    Task<IEnumerable<IError>> UserExistsAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> GetUserAsync(Guid userID, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserDtoAsync(Guid userID, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> GetUserProfileDtoAsync(Guid userID, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> GetUserProfileDtoByNameAsync(string username, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyCollection<UserDto>> GetUserFollowersAsync(Guid userID, CancellationToken ct = default);
    Task<IReadOnlyCollection<UserDto>> GetUserFollowingAsync(Guid userID, CancellationToken ct = default);
    
    Task<IPagedList<UserDto>> SearchByUsernameAsync(string username, int page, int pageSize, CancellationToken ct = default);
    
    Task AddAsync(User user);
    
    Task UpdateAsync(User user);
}