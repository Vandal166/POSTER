using System.Security.Claims;
using Application.DTOs;
using FluentResults;
using Microsoft.AspNetCore.Authentication;

namespace Infrastructure.Auth;

public interface IAuthService
{
    // at the end automatically syncs the keycloak user to the local database
    Task<Result> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default);
    
    Task<Result<(ClaimsPrincipal Principal, AuthenticationProperties Props)>> LoginAsync(LoginUserDto dto, CancellationToken ct = default);

    Task<Result> UpdateKeycloakUsernameAsync(string userID, UsernameDto dto, CancellationToken ct = default);

    Task<Result> UpdateKeycloakAvatarAsync(string userID, string avatarPath, CancellationToken ct = default);
}