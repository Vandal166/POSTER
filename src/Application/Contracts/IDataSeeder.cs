namespace Application.Contracts;

public interface IDataSeeder
{
    /// <summary>
    /// Seed N users, each with M posts and K comments
    /// </summary>
    Task SeedAsync(int userCount, int postsPerUser, int commentsPerPost, CancellationToken ct = default);
}