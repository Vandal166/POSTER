using Application.DTOs;
using FluentResults;

namespace Application.Contracts;

public interface IAuthService
{
    Task<Result<Guid>> RegisterAsync(RegisterUserDto dto);
    Task<Result<string>> LoginAsync(LoginUserDto dto);
}