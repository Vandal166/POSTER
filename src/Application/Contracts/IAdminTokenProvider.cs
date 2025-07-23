namespace Application.Contracts;

public interface IAdminTokenProvider
{
    // returns a valid Keycloak admin access token.
    Task<string> GetTokenAsync(CancellationToken ct = default);
}