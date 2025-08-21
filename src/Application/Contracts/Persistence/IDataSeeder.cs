namespace Application.Contracts.Persistence;

public interface IDataSeeder
{
    /// <summary>
    /// Seed N users, each with M posts and K comments
    /// </summary>
    Task SeedAsync(int userCount, CancellationToken ct = default);
}