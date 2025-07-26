using Application.Contracts;
using Application.Contracts.Persistence;
using Domain.Entities;

namespace Infrastructure.Auth;

public class UserSynchronizer : IUserSynchronizer
{
    private readonly IKeycloakUserService _kc;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public UserSynchronizer(IKeycloakUserService kc, IUserRepository users, IUnitOfWork uow)
    {
        _kc = kc;
        _users = users;
        _uow = uow;
    }

    public async Task SyncAsync(string keycloakID, CancellationToken ct = default)
    {
        var kcUser = await _kc.GetUserAsync(keycloakID, ct)
                     ?? throw new InvalidOperationException("Keycloak user not found.");

        if (!Guid.TryParse(kcUser.ID, out var id))
            throw new InvalidOperationException("Invalid Keycloak user ID.");

        var existing = await _users.GetUserAsync(id, ct);
        if (existing is null)
        {
            var user = User.Create(id, kcUser.Username).Value;
            await _users.AddAsync(user); 
        }
        else // if the user exists then we update him to reflect Keycloak changes
        {
            existing.Username = kcUser.Username!;
            existing.AvatarPath = kcUser.Attributes["avatarPath"]?.FirstOrDefault() ?? existing.AvatarPath;
            await _users.UpdateAsync(existing);
        }

        await _uow.SaveChangesAsync(ct);
    }
}