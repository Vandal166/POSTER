namespace Application.Contracts.Auth;

public interface ISessionValidator
{
    // checks if the provided token is still active
    Task<bool> IsTokenActiveAsync(string token, CancellationToken ct = default);
}