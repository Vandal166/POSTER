using Application.DTOs;

namespace Application.Contracts.Persistence;

public interface IAvatarService
{
    // creates locally the uploaded avatar and returns the relative PATH to the avatar image ie "/uploads/avatars/12345.png"
    Task<string> UpdateAvatarAsync(string userID, AvatarDto file, CancellationToken ct = default);
}
