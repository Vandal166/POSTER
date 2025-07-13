namespace Application.Contracts;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string plain, string hash);
}