using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IUserRepository
{
    Task<IEnumerable<IError>> UserExistsAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> GetUserAsync(Guid userID, CancellationToken cancellationToken = default);
    Task<User?> GetUserAsync(UserName userName, CancellationToken cancellationToken = default);
    Task<User?> GetUserAsync(Email userEmail, CancellationToken cancellationToken = default);
    
    Task AddAsync(User user);
    
    Task UpdateAsync(User user);
}