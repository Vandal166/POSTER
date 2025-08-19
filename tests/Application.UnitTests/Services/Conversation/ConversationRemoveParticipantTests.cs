using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.Services;

public class ConversationRemoveParticipantTests
{
    private readonly ConversationServiceFixture _fixture = new();
    
    [Fact]
    public async Task RemoveParticipantAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var conversationID = Guid.NewGuid();
        var participantID = Guid.NewGuid();
        var currentUserID = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var conversation = Conversation.Create("Test Conversation", Guid.NewGuid(), currentUserID).Value;

        _fixture.ConversationRepositoryMock.ExistsAsync(conversationID, cancellationToken)
            .Returns(Task.FromResult(true));

        _fixture.ConversationRepositoryMock.GetConversationAsync(conversationID, currentUserID, cancellationToken)
            .Returns(conversation);

        _fixture.ConversationRepositoryMock.GetConversationParticipantAsync(conversationID, participantID, cancellationToken)
            .Returns(new ConversationUser { Conversation = conversation, UserID = participantID });
        // Act
        var result = await _fixture.ConversationService.RemoveParticipantAsync(conversationID, participantID, currentUserID, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        await _fixture.ConversationRepositoryMock.Received(1)
            .DeleteParticipantAsync(Arg.Any<ConversationUser>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWorkMock.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveParticipantAsync_WithNonExistentConversation_ShouldReturnFailResult()
    {
        // Arrange
        var conversationID = Guid.NewGuid();
        var participantID = Guid.NewGuid();
        var currentUserID = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        _fixture.ConversationRepositoryMock.ExistsAsync(conversationID, cancellationToken)
            .Returns(Task.FromResult(false));
        
        // Act
        var result = await _fixture.ConversationService.RemoveParticipantAsync(conversationID, participantID, currentUserID, cancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("Conversation does not exist.", result.Errors[0].Message);
        await _fixture.ConversationRepositoryMock.DidNotReceive()
            .GetConversationAsync(conversationID, currentUserID, cancellationToken);
        await _fixture.ConversationRepositoryMock.DidNotReceive()
            .GetConversationParticipantAsync(conversationID, participantID, cancellationToken);
        await _fixture.ConversationRepositoryMock.DidNotReceive()
            .DeleteParticipantAsync(Arg.Any<ConversationUser>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWorkMock.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task RemoveParticipantAsync_WithNonExistentParticipant_ShouldReturnFailResult()
    {
        // Arrange
        var conversationID = Guid.NewGuid();
        var participantID = Guid.NewGuid();
        var currentUserID = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        _fixture.ConversationRepositoryMock.ExistsAsync(conversationID, cancellationToken)
            .Returns(Task.FromResult(true));
        
        _fixture.ConversationRepositoryMock.GetConversationAsync(conversationID, currentUserID, cancellationToken)
            .Returns(Conversation.Create("Test Conversation", Guid.NewGuid(), currentUserID).Value);
        
        _fixture.ConversationRepositoryMock.GetConversationParticipantAsync(conversationID, participantID, cancellationToken)
            .Returns((ConversationUser?)null); // Participant does not exist
        
        // Act
        var result = await _fixture.ConversationService.RemoveParticipantAsync(conversationID, participantID, currentUserID, cancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("Participant not found in this conversation.", result.Errors[0].Message);
        
        await _fixture.ConversationRepositoryMock.DidNotReceive()
            .DeleteParticipantAsync(Arg.Any<ConversationUser>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWorkMock.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}