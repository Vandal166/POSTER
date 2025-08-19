namespace Domain.UnitTests;

public class MessageTests
{
    [Fact]
    public void CreateMessage_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var senderID = Guid.NewGuid();
        var conversationID = Guid.NewGuid();
        var content = "This is a test message.";
        
        // Act
        var result = Message.Create(senderID, conversationID, content);
        
        // Assert
        Assert.True(result.IsSuccess, "Message creation should succeed with valid data.");
        Assert.NotNull(result.Value);
        Assert.Equal(senderID, result.Value.SenderID);
        Assert.Equal(conversationID, result.Value.ConversationID);
        Assert.Equal(content, result.Value.Content);
        Assert.NotEqual(Guid.Empty, result.Value.ID);
        Assert.NotEqual(DateTime.MinValue, result.Value.CreatedAt);
    }

    [Fact]
    public void CreateMessage_WithEmptySenderID_ShouldReturnFailResult()
    {
        // Arrange
        var senderID = Guid.Empty; // Invalid sender ID
        var conversationID = Guid.NewGuid();
        var content = "This is a test message.";

        // Act
        var result = Message.Create(senderID, conversationID, content);

        // Assert
        Assert.False(result.IsSuccess, "Message creation should fail with empty sender ID.");
        Assert.Equal("Sender and recipient IDs cannot be empty.", result.Errors[0].Message);
    }

    [Fact]
    public void CreateMessage_WithEmptyConversationID_ShouldReturnFailResult()
    {
        // Arrange
        var senderID = Guid.NewGuid();
        var conversationID = Guid.Empty; // Invalid conversation ID
        var content = "This is a test message.";

        // Act
        var result = Message.Create(senderID, conversationID, content);

        // Assert
        Assert.False(result.IsSuccess, "Message creation should fail with empty conversation ID.");
        Assert.Equal("Sender and recipient IDs cannot be empty.", result.Errors[0].Message);
    }

    [Fact]
    public void CreateSystemMessage_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var conversationID = Guid.NewGuid();
        var content = "This is a system message.";
        
        // Act
        var result = Message.CreateSystemMessage(conversationID, content);
        
        // Assert
        Assert.True(result.IsSuccess, "System message creation should succeed with valid data.");
        Assert.NotNull(result.Value);
        Assert.Equal(Message.SystemUserId, result.Value.SenderID);
        Assert.Equal(conversationID, result.Value.ConversationID);
        Assert.Equal(content, result.Value.Content);
        Assert.NotEqual(Guid.Empty, result.Value.ID);
        Assert.NotEqual(DateTime.MinValue, result.Value.CreatedAt);
    }

    [Fact]
    public void CreateSystemMessage_WithEmptyConversationID_ShouldReturnFailResult()
    {
        // Arrange
        var conversationID = Guid.Empty; // Invalid conversation ID
        var content = "This is a system message.";
        
        // Act
        var result = Message.CreateSystemMessage(conversationID, content);
        
        // Assert
        Assert.False(result.IsSuccess, "System message creation should fail with empty conversation ID.");
        Assert.Equal("Conversation ID cannot be empty.", result.Errors[0].Message);
    }
}