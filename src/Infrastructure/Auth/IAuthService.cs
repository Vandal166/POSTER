using System.Security.Claims;
using Application.DTOs;
using FluentResults;
using Microsoft.AspNetCore.Authentication;

namespace Infrastructure.Auth;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default);
    
    Task<Result<(ClaimsPrincipal Principal, AuthenticationProperties Props)>> LoginAsync(LoginUserDto dto, CancellationToken ct = default);

    Task<Result> UpdateKeycloakProfileAsync(string userID, UsernameDto dto, CancellationToken ct = default);
}