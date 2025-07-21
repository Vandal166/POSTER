using System.Security.Claims;
using Application.DTOs;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Auth;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken);
    Task<Result<(ClaimsPrincipal Principal, AuthenticationProperties Props)>> LoginAsync(LoginUserDto dto, HttpContext httpContext);

    Task<Result> CompleteProfileAsync(string userID, string userName);
}