namespace Application.UnitTests.Services;

public class PostServiceDeleteTests
{
    private readonly PostServiceFixture _fixture = new();
    
    [Fact]
    public async Task DeletePostAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var currentUserId = Guid.NewGuid();
        var post = Post.Create(currentUserId, "Sample content", null).Value;
        _fixture.PostRepositoryMock.GetPostByIDAsync(post.ID, cancellationToken)
            .Returns(post);
        _fixture.PostRepositoryMock.DeleteAsync(post, cancellationToken)
            .Returns(Task.CompletedTask);
        _fixture.UowMock.SaveChangesAsync(cancellationToken)
            .Returns(Task.CompletedTask);
        
        var postId = post.ID;
        
        // Act
        var result = await _fixture.PostService.DeletePostAsync(postId, currentUserId, cancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        await _fixture.PostRepositoryMock.Received(1)
            .GetPostByIDAsync(postId, cancellationToken);
        await _fixture.PostRepositoryMock.Received(1)
            .DeleteAsync(post, cancellationToken);
        await _fixture.UowMock.Received(1)
            .SaveChangesAsync(cancellationToken);
    }
    
    [Fact]
    public async Task DeletePostAsync_WithInvalidId_ShouldReturnFailResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var currentUserId = Guid.NewGuid();
        _fixture.PostRepositoryMock.GetPostByIDAsync(Arg.Any<Guid>(), cancellationToken)
            .Returns((Post?)null);
        var postId = Guid.NewGuid();
        
        // Act
        var result = await _fixture.PostService.DeletePostAsync(postId, currentUserId, cancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Post not found");
        await _fixture.PostRepositoryMock.Received(1)
            .GetPostByIDAsync(postId, cancellationToken);
        await _fixture.PostRepositoryMock.DidNotReceive()
            .DeleteAsync(Arg.Any<Post>(), cancellationToken);
        await _fixture.UowMock.DidNotReceive()
            .SaveChangesAsync(cancellationToken);
    }
    
    [Fact]
    public async Task DeletePostAsync_WithInvalidUserId_ShouldReturnFailResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var currentUserId = Guid.NewGuid();
        var post = Post.Create(Guid.NewGuid(), "Sample content", null).Value; // Different user ID
        _fixture.PostRepositoryMock.GetPostByIDAsync(post.ID, cancellationToken)
            .Returns(post);
        
        var postId = post.ID;
        
        // Act
        var result = await _fixture.PostService.DeletePostAsync(postId, currentUserId, cancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Unable to delete post that does not belong to you");
        await _fixture.PostRepositoryMock.Received(1)
            .GetPostByIDAsync(postId, cancellationToken);
        await _fixture.PostRepositoryMock.DidNotReceive()
            .DeleteAsync(Arg.Any<Post>(), cancellationToken);
        await _fixture.UowMock.DidNotReceive()
            .SaveChangesAsync(cancellationToken);
    }
}