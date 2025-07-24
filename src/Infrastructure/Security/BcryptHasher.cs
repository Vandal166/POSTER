using Application.Contracts;
using Application.Contracts.Auth;

namespace Infrastructure.Security;

public class BcryptHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string plain, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(plain, hash);
    }
}