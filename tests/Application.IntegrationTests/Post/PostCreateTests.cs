using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests;

public class PostCreateTests : BaseIntegrationTest
{
    public PostCreateTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePost_ShouldAdd_NewPostToDatabase()
    {
        // Arrange
        using var scope = _serviceScope.ServiceProvider.CreateScope();  
        var postService = scope.ServiceProvider.GetRequiredService<IPostService>();
        var user = DataSeeder.GetTestUser();
        var post = DataSeeder.GetTestPost();
        
        var createPostDto = new CreatePostDto
        (
            post.Content,
            post.VideoFileID,
            post.Images.Select(i => i.ImageFileID).ToArray()
        );
        
        // Act
        var result = await postService.CreatePostAsync(createPostDto, user.ID, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        var createdPost = await DbContext.Posts
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ID == result.Value);

        Assert.NotNull(createdPost);
        Assert.Equal(createPostDto.Content, createdPost.Content);
        Assert.Equal(user.ID, createdPost.AuthorID);
        Assert.Equal(createPostDto.VideoFileID, createdPost.VideoFileID);
        Assert.Equal(createPostDto.ImageFileIDs.Length, createdPost.Images.Count);
    }
    
    [Fact]
    public async Task CreatePost_ShouldReturn_ForeignKeyViolation_WhenUserDoesNotExist()
    {
        // Arrange
        using var scope = _serviceScope.ServiceProvider.CreateScope();  
        var postService = scope.ServiceProvider.GetRequiredService<IPostService>();
        
        var createPostDto = new CreatePostDto("Test content", null, Array.Empty<Guid>());
        
        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            postService.CreatePostAsync(createPostDto, Guid.NewGuid(), CancellationToken.None));
    }
}