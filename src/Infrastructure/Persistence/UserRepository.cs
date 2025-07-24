using Application.Contracts;
using Application.Contracts.Persistence;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

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
    
    public async Task<User?> GetUserAsync(Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.ID == userID, cancellationToken: cancellationToken);
    }

    public async Task<User?> GetUserAsync(UserName username, CancellationToken cancellationToken = default)
    {
        /*return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.Value == username.Value, cancellationToken: cancellationToken);*/ return null;
    }
    public async Task<User?> GetUserAsync(Email userEmail, CancellationToken cancellationToken = default)
    {
        /*return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.Value == userEmail.Value, cancellationToken: cancellationToken);*/ return null;
    }
}