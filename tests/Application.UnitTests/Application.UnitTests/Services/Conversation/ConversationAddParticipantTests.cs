using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.Services;

public class ConversationAddParticipantTests
{
    private readonly ConversationServiceFixture _fixture = new();
    
    [Fact]
    public async Task AddParticipantsAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var currentUserID = Guid.NewGuid();
        var conversation = Conversation.Create("Test Conversation", Guid.NewGuid(), currentUserID).Value;
        var conversationID = conversation.ID;
        List<Guid> participantIDs = new() { Guid.NewGuid(), Guid.NewGuid() };
        _fixture.ConversationRepositoryMock.ExistsAsync(conversationID, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _fixture.ConversationRepositoryMock
            .GetConversationAsync(conversationID, currentUserID, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _fixture.ConversationRepositoryMock
            .AddParticipantsAsync(Arg.Any<ConversationUser>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fixture.UnitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _fixture.ConversationService.AddParticipantAsync(conversationID, participantIDs, currentUserID);
        
        // Assert
        Assert.True(result.IsSuccess);
        await _fixture.ConversationRepositoryMock.Received(participantIDs.Count)
            .AddParticipantsAsync(Arg.Is<ConversationUser>(c => 
                c.ConversationID == conversationID && 
                participantIDs.Contains(c.UserID)), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWorkMock.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }   
    
    [Fact]
    public async Task AddParticipantsAsync_WithNoNewParticipants_ShouldReturnFailResult()
    {
        // Arrange
        var currentUserID = Guid.NewGuid();
        var conversationID = Guid.NewGuid();
        var conversation = Conversation.Create("Test Conversation", Guid.NewGuid(), currentUserID).Value;
        
        _fixture.ConversationRepositoryMock.ExistsAsync(conversationID, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _fixture.ConversationRepositoryMock.GetConversationAsync(conversationID, currentUserID, Arg.Any<CancellationToken>())
            .Returns(conversation);
        
        List<Guid> participantIDs = new() { currentUserID }; // Trying to add the current user as a participant
        _fixture.ConversationRepositoryMock.AddParticipantsAsync(Arg.Any<ConversationUser>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fixture.UnitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _fixture.ConversationService.AddParticipantAsync(conversationID, participantIDs, currentUserID);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No new participants to add.", result.Errors[0].Message);
        await _fixture.ConversationRepositoryMock.DidNotReceive()
            .AddParticipantsAsync(Arg.Any<ConversationUser>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWorkMock.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

}