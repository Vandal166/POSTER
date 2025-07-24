using Bogus;
using Domain.Entities;
using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.ValueObjects;

namespace Infrastructure.Seeding;


/// <summary>
/// Used for seeding fake data into the database.
/// </summary>
public class DataSeeder : IDataSeeder
{
    private readonly IUserRepository _users;
    private readonly IPostRepository _posts;
    private readonly IPostCommentRepository _comments;
    private readonly IUnitOfWork _uow;

    public DataSeeder(
        IUserRepository users,
        IPostRepository posts,
        IPostCommentRepository comments,
        IUnitOfWork uow)
    {
        _users    = users;
        _posts    = posts;
        _comments = comments;
        _uow      = uow;
    }

    public async Task SeedAsync(int userCount, int postsPerUser, int commentsPerPost, CancellationToken ct = default)
    {
        /*// Faker for Users
        var userFaker = new Faker<User>()
            .CustomInstantiator(f =>
                User.Create(
                    UserName.Create(f.Internet.UserName()).Value,
                    Email.Create(f.Internet.Email()).Value,
                    BCrypt.Net.BCrypt.HashPassword("Password123!")  // fixed dev password
                ).Value);

        // Generate and persist users
        var users = userFaker.Generate(userCount);
        foreach (var user in users)
        {
            await _users.AddAsync(user);
        }
        await _uow.SaveChangesAsync(ct);

        // Faker for Posts
        var postFaker = new Faker<Post>()
            .CustomInstantiator(f => 
                Post.Create(f.PickRandom(users).ID,f.Lorem.Sentence()).Value);

        // Generate and persist posts
        var posts = postFaker.Generate(postsPerUser * userCount);
        foreach (var post in posts)
        {
            await _posts.AddAsync(post, ct);
        }
        await _uow.SaveChangesAsync(ct);

        // Faker for Comments
        var commentFaker = new Faker<Comment>()
            .CustomInstantiator(f =>
                Comment.Create( f.PickRandom(posts).ID, f.PickRandom(users).ID, content: f.Lorem.Sentence()).Value);

        // Generate and persist comments
        var comments = commentFaker.Generate(commentsPerPost * posts.Count);
        foreach (var comment in comments)
        {
            await _comments.AddAsync(comment, ct);
        }
        await _uow.SaveChangesAsync(ct);*/
    }
}
