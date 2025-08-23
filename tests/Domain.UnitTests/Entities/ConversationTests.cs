namespace Domain.UnitTests;

public class ConversationTests
{
    [Fact]
    public void CreateConversationUser_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var conversationName = "Test Conversation";
        var conversationProfilePictureID = Guid.NewGuid();
        var createdByID = Guid.NewGuid();

        // Act
        var result = Conversation.Create(conversationName, conversationProfilePictureID, createdByID);

        // Assert
        Assert.True(result.IsSuccess, "Conversation creation should succeed with valid data.");
        Assert.NotNull(result.Value);
        Assert.Equal(conversationName, result.Value.Name);
        Assert.Equal(conversationProfilePictureID, result.Value.ProfilePictureID);
        Assert.Equal(createdByID, result.Value.CreatedByID);
    }

    [Fact]
    public void CreateConversationUser_WithEmptyName_ShouldReturnFailResult()
    {
        // Arrange
        var conversationName = ""; // Invalid name
        var conversationProfilePictureID = Guid.NewGuid();
        var createdByID = Guid.NewGuid();

        // Act
        var result = Conversation.Create(conversationName, conversationProfilePictureID, createdByID);

        // Assert
        Assert.False(result.IsSuccess, "Conversation creation should fail with empty name.");
        Assert.Equal("Conversation name cannot be empty.", result.Errors[0].Message);
    }

    [Fact]
    public void CreateConversationUser_WithEmptyProfilePictureID_ShouldReturnOkResult()
    {
        // Arrange
        var conversationName = "Test Conversation";
        var conversationProfilePictureID = Guid.Empty; // Optional profile picture ID
        var createdByID = Guid.NewGuid();
    
        // Act
        var result = Conversation.Create(conversationName, conversationProfilePictureID, createdByID);
    
        // Assert
        Assert.True(result.IsSuccess, "Conversation creation should succeed with empty profile picture ID.");
        Assert.NotNull(result.Value);
        Assert.Equal(conversationProfilePictureID, result.Value.ProfilePictureID);
    }

    [Fact]
    public void CreateConversationUser_WithEmptyCreatedByID_ShouldReturnFailResult()
    {
        // Arrange
        var conversationName = "Test Conversation";
        var conversationProfilePictureID = Guid.NewGuid();
        var createdByID = Guid.Empty; // Invalid created by ID

        // Act
        var result = Conversation.Create(conversationName, conversationProfilePictureID, createdByID);

        // Assert
        Assert.False(result.IsSuccess, "Conversation creation should fail with empty created by ID.");
        Assert.Equal("Created by user ID cannot be empty.", result.Errors[0].Message);
    }
    
    [Fact]
    public void UpdateConversation_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var conversation = Conversation.Create("Initial Name", Guid.NewGuid(), Guid.NewGuid()).Value;
        var newName = "Updated Conversation";
        var newProfilePictureID = Guid.NewGuid();
        
        // Act
        var result = Conversation.Update(conversation, newName, newProfilePictureID);
        
        // Assert
        Assert.True(result.IsSuccess, "Conversation update should succeed with valid data.");
        Assert.NotNull(result.Value);
        Assert.Equal(newName, result.Value.Name);
        Assert.Equal(newProfilePictureID, result.Value.ProfilePictureID);
        Assert.Equal(conversation.UpdatedAt, result.Value.UpdatedAt);
        Assert.NotNull(result.Value.UpdatedAt);
    }
}