namespace Application.UnitTests.Services;

public class PostServiceGetTests
{
    private readonly PostServiceFixture _fixture = new();
    
    [Fact]
    public async Task GetPostAsync_WithValidId_ShouldReturnPost()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var post = Post.Create(Guid.NewGuid(), "Sample content", null).Value;
        _fixture.PostRepositoryMock.GetPostByIDAsync(post.ID, cancellationToken)
            .Returns(post);
        var postId = post.ID;
        
        // Act
        var result = await _fixture.PostService.GetPostAsync(postId, cancellationToken);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(postId, result?.ID);
        await _fixture.PostRepositoryMock.Received(1)
            .GetPostByIDAsync(postId, cancellationToken);
    }
        
    [Fact]
    public async Task GetPostAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _fixture.PostRepositoryMock.GetPostByIDAsync(Arg.Any<Guid>(), cancellationToken)
            .Returns((Post?)null);
        var postId = Guid.NewGuid();
        
        // Act
        var result = await _fixture.PostService.GetPostAsync(postId, cancellationToken);
        
        // Assert
        Assert.Null(result);
        await _fixture.PostRepositoryMock.Received(1)
            .GetPostByIDAsync(postId, cancellationToken);
    }
}