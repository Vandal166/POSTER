namespace Domain.UnitTests;

public class PostTests
{
    [Fact]
    public void CreatePost_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var authorID = Guid.NewGuid();
        var content = "This is a test post.";
        
        // Act
        var result = Post.Create(authorID, content);
        
        // Assert
        Assert.True(result.IsSuccess, "Post creation should succeed with valid data.");
        Assert.NotNull(result.Value);
        Assert.Equal(authorID, result.Value.AuthorID);
        Assert.Equal(content, result.Value.Content);
    }

    [Fact]
    public void CreatePost_WithEmptyAuthorID_ShouldReturnFailResult()
    {
        // Arrange
        var authorID = Guid.Empty; // Invalid author ID
        var content = "This is a test post.";

        // Act
        var result = Post.Create(authorID, content);

        // Assert
        Assert.False(result.IsSuccess, "Post creation should fail with empty author ID.");
        Assert.Equal("Author ID cannot be empty.", result.Errors[0].Message);
    }
}