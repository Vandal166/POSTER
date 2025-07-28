using Application.DTOs;
using Domain.Constants;
using FluentResults;

namespace Infrastructure.Auth;

public interface IKeycloakUserService
{
    // Creates a new Keycloak user and returns its Keycloak-generated ID.
    Task<IResult<Guid>> CreateAsync(RegisterUserDto dto, string adminToken, CancellationToken ct = default);

    Task<IResult<string>> GetAsync(Guid userID, string adminToken, CancellationToken ct = default);

    // returns an KeycloakUser obj instead of just the ID
    Task<KeycloakUser?> GetUserAsync(Guid userID, CancellationToken ct = default);
    
    Task<IResult<Guid>> UpdateUsernameAsync(Guid userID, string userName, string userJson, string adminToken, CancellationToken ct = default);
    
    Task<IResult<Guid>> UpdateAvatarAsync(Guid userID, string avatarPath, string userJson, string adminToken, CancellationToken ct = default);
}