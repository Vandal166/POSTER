using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Contracts;

public interface IUserRepository
{
    Task<IEnumerable<IError>> UserExistsAsync(User user);
    Task<User?> GetUserByLogin(UserName userName);
    Task AddAsync(User user);
}