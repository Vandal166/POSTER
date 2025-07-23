using Application.DTOs;
using Domain.Constants;
using FluentResults;

namespace Infrastructure.Auth;

public interface IKeycloakUserService
{
    // Creates a new Keycloak user and returns its Keycloak-generated ID.
    Task<IResult<string>> CreateAsync(RegisterUserDto dto, string adminToken, CancellationToken ct = default);

    Task<IResult<string>> GetAsync(string userID, string adminToken, CancellationToken ct = default);

    // returns an KeycloakUser obj instead of just the ID
    Task<KeycloakUser?> GetUserAsync(string userID, CancellationToken ct = default);
    
    Task<IResult<string>> UpdateAsync(string userID, string userName, string userJson, string adminToken, CancellationToken ct = default);
}