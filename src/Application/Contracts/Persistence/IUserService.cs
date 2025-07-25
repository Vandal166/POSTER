using Application.DTOs;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IUserService
{
    //TODO when deleting a user, delete the avatar file as well and other related data
    Task<string> GetAvatarPathAsync(string userID, CancellationToken ct = default);
    
    Task<Result> UpdateUsernameAsync(string userID, UsernameDto dto, CancellationToken ct = default);

    Task<Result<string>> UpdateAvatarAsync(string userID, AvatarDto fileDto, CancellationToken ct = default);
}