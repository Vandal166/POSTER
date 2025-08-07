using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using Infrastructure.Common;
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