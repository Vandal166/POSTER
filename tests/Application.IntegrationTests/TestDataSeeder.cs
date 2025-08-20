using Bogus;
using Domain.Entities;
using Infrastructure.Data;

namespace Application.IntegrationTests;

public class TestDataSeeder
{
    private readonly PosterDbContext _context;
    private readonly Faker _faker;
    
    private readonly List<User> _users = new();
    private readonly List<Post> _posts = new();

    public User GetTestUser(int index = 0) => _users[index];
    public Post GetTestPost(int index = 0) => _posts[index];
    
    
    public TestDataSeeder(PosterDbContext context)
    {
        _context = context;
        _faker = new Faker();
    }

    public async Task SeedAsync()
    {
        await SeedUsersAsync();
        await SeedPostsAsync();
        
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedUsersAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            var user = User.Create(Guid.NewGuid(), _faker.Internet.UserName()).Value;
            _users.Add(user);
            await _context.Users.AddAsync(user);
        }
    }
    
    private async Task SeedPostsAsync()
    {
        foreach (var user in _users)
        {
            for (int i = 0; i < _faker.Random.Int(1, 5); i++)
            {
                var post = Post.Create(user.ID, _faker.Lorem.Sentence(), null).Value;
                _posts.Add(post);
                await _context.Posts.AddAsync(post);
                
                // adding rnd amount of images to the post
                for (int j = 0; j < _faker.Random.Int(1, 3); j++)
                {
                    var imageID = Guid.NewGuid();
                    var postImage = PostImage.Create(post.ID, imageID).Value;
                    await _context.PostImages.AddAsync(postImage);
                }
            }
        }
    }
}