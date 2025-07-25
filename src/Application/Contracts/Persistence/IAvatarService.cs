using Application.DTOs;

namespace Application.Contracts.Persistence;

public interface IAvatarService
{
    // returns the PATH to the avatar image
    Task<string> UpdateAvatarAsync(string userID, AvatarDto file, CancellationToken ct = default);
}
