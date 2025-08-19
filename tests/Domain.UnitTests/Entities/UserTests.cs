namespace Domain.UnitTests;

public class UserTests
{
    [Fact]
    public void CreateUser_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var keycloakID = Guid.NewGuid();
        var username = "testuser";
        
        // Act
        var result = User.Create(keycloakID, username);
        
        // Assert
        Assert.True(result.IsSuccess, "User creation should succeed with valid data.");
        Assert.NotNull(result.Value);
        Assert.Equal(keycloakID, result.Value.ID);
        Assert.Equal(username, result.Value.Username);
    }

    [Fact]
    public void CreateUser_WithInvalidUsername_ShouldReturnFailResult()
    {
        // Arrange
        var keycloakID = Guid.NewGuid();
        var username = "ab"; // Invalid username (too short)
        
        // Act
        var result = User.Create(keycloakID, username);
        
        // Assert
        Assert.False(result.IsSuccess, "User creation should fail with invalid username.");
        Assert.Equal("Username must be between 3 and 50 characters long.", result.Errors[0].Message);
    }
}