using System.Security.Claims;
using Application.DTOs;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Auth;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterUserDto dto, CancellationToken ct);
    Task<Result<(ClaimsPrincipal Principal, AuthenticationProperties Props)>> LoginAsync(LoginUserDto dto, CancellationToken ct = default);

    Task<Result> CompleteProfileAsync(string userID, CompleteProfileDto dto, CancellationToken ct = default);
}