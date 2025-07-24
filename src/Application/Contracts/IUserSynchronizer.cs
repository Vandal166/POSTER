namespace Application.Contracts;

public interface IUserSynchronizer
{
    // syncs the Keycloak user to the local database.
    Task SyncAsync(string keycloakID, CancellationToken ct = default);
}