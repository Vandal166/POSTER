namespace Application.Contracts;

public interface IUserSynchronizer
{
    // syncs the Keycloak user to the local database, calls SaveChangesAsync on the UnitOfWork
    Task SyncAsync(string keycloakID, CancellationToken ct = default);
}