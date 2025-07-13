using Application.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly PosterDbContext _db;
    public UserRepository(PosterDbContext db) => _db = db;

    public async Task<IEnumerable<IError>> UserExistsAsync(User user)
    {
        var errors = new List<IError>();
        if (await _db.Users.AnyAsync(u => u.Username.Value == user.Username.Value))
        {
            errors.Add(new Error($"Username '{user.Username.Value}' already exists."));
        }
        
        if (await _db.Users.AnyAsync(u => u.Email.Value == user.Email.Value))
        {
            errors.Add(new Error($"Email '{user.Email.Value}' already exists."));
        }
        return errors;
    }

    public async Task<User?> GetUserByLogin(UserName userName)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.Value == userName.Value);
    }

    public Task AddAsync(User user)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }
}