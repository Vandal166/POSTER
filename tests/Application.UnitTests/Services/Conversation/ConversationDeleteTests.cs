using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.Services;

public class ConversationDeleteTests
{
    private readonly ConversationServiceFixture _fixture = new();
    
    [Fact]
    public async Task DeleteConversationAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var currentUserID = Guid.NewGuid();
        var conversation = Conversation.Create("Test Conversation", Guid.NewGuid(), currentUserID).Value;
        var conversationID = conversation.ID;
        _fixture.ConversationRepositoryMock.ExistsAsync(conversationID, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _fixture.ConversationRepositoryMock
            .GetConversationAsync(conversationID, currentUserID, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _fixture.ConversationRepositoryMock
            .DeleteAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fixture.UnitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fixture.BlobServiceMock
            .DeleteFileAsync(conversation.ProfilePictureID.GetValueOrDefault(), "images", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _fixture.ConversationService.DeleteConversationAsync(conversationID, currentUserID);
        
        // Assert
        Assert.True(result.IsSuccess);
        await _fixture.ConversationRepositoryMock.Received(1).DeleteAsync(Arg.Is<Conversation>(c => 
            c.ID == conversationID && 
            c.CreatedByID == currentUserID), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _fixture.BlobServiceMock.Received(1).DeleteFileAsync(conversation.ProfilePictureID.GetValueOrDefault(), "images", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteConversationAsync_WithNonExistentConversation_ShouldReturnFailResult()
    {
        // Arrange
        var currentUserID = Guid.NewGuid();
        var conversationID = Guid.NewGuid();
        _fixture.ConversationRepositoryMock.ExistsAsync(conversationID, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _fixture.ConversationRepositoryMock
            .GetConversationAsync(conversationID, currentUserID, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);
        _fixture.UnitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fixture.BlobServiceMock
            .DeleteFileAsync(Arg.Any<Guid>(), "images", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _fixture.ConversationService.DeleteConversationAsync(conversationID, currentUserID);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Conversation does not exist.", result.Errors[0].Message);
        await _fixture.ConversationRepositoryMock.DidNotReceive().DeleteAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>());
    }
}