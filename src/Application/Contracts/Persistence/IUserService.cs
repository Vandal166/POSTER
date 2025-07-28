using Application.DTOs;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IUserService
{
    //TODO when deleting a user, delete the avatar file as well and other related data
    Task<Result<string>> GetAvatarPathAsync(Guid userID, CancellationToken ct = default);
    
    Task<Result> UpdateUsernameAsync(Guid userID, UsernameDto dto, CancellationToken ct = default);

    Task<Result> UpdateAvatarAsync(Guid userID, AvatarDto fileDto, CancellationToken ct = default);
}