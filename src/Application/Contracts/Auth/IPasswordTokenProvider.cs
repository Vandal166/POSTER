using Application.DTOs;
using FluentResults;

namespace Application.Contracts.Auth;

public interface IPasswordTokenProvider
{
    // Exchanges username/password for tokens and extracts the userID ("sub") claim from the access token.
    Task<IResult<TokenResponse>> ExchangeAsync(string login, string password, CancellationToken ct = default);
}