using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.Services;

public class ConversationLeaveParticipantTests
{
    private readonly ConversationServiceFixture _fixture = new();

    [Fact]
    public async Task LeaveParticipantAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var conversationID = Guid.NewGuid();
        var currentUserID = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        _fixture.ConversationRepositoryMock.ExistsAsync(conversationID, cancellationToken)
            .Returns(Task.FromResult(true));
        _fixture.ConversationRepositoryMock.GetConversationAsync(conversationID, currentUserID, cancellationToken)
            .Returns(Conversation.Create("Test Conversation", Guid.NewGuid(), currentUserID).Value);
        _fixture.ConversationRepositoryMock.GetConversationParticipantAsync(conversationID, currentUserID, cancellationToken)
            .Returns(new ConversationUser { ConversationID = conversationID, UserID = currentUserID });
        
        _fixture.ConversationRepositoryMock.DeleteParticipantAsync(Arg.Any<ConversationUser>(), cancellationToken)
            .Returns(Task.CompletedTask);
        _fixture.UnitOfWorkMock.SaveChangesAsync(cancellationToken)
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _fixture.ConversationService.LeaveConversationAsync(conversationID, currentUserID, cancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        await _fixture.ConversationRepositoryMock.Received(1)
            .DeleteParticipantAsync(Arg.Is<ConversationUser>(c => c.ConversationID == conversationID && c.UserID == currentUserID), cancellationToken);
        await _fixture.UnitOfWorkMock.Received(1)
            .SaveChangesAsync(cancellationToken);
    }
    
    [Fact]
    public async Task LeaveParticipantAsync_WhileNotBeingAnParticipant_ShouldReturnFailResult()
    {
        // Arrange
        var conversationID = Guid.NewGuid();
        var currentUserID = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        _fixture.ConversationRepositoryMock.ExistsAsync(conversationID, cancellationToken)
            .Returns(Task.FromResult(true));
        _fixture.ConversationRepositoryMock.GetConversationAsync(conversationID, currentUserID, cancellationToken)
            .Returns(Conversation.Create("Test Conversation", Guid.NewGuid(), currentUserID).Value);
        _fixture.ConversationRepositoryMock.GetConversationParticipantAsync(conversationID, currentUserID, cancellationToken)
            .Returns((ConversationUser?)null); // User is not a participant
        
        // Act
        var result = await _fixture.ConversationService.LeaveConversationAsync(conversationID, currentUserID, cancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("You are not a participant in this conversation.", result.Errors[0].Message);
        
        await _fixture.ConversationRepositoryMock.DidNotReceive()
            .DeleteParticipantAsync(Arg.Any<ConversationUser>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWorkMock.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}