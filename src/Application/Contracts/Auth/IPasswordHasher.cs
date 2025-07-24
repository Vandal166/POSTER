namespace Application.Contracts.Auth;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string plain, string hash);
}