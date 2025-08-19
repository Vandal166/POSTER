using Application.DTOs;
using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.Services;

public class ConversationUpdateTests
{
    private readonly ConversationServiceFixture _fixture = new();
    
    [Fact]
    public async Task UpdateConversationAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var updateDto = new UpdateConversationDto
        (
            Guid.NewGuid(),
            "Updated Conversation",
            Guid.NewGuid()
        );
        var currentUserID = Guid.NewGuid();
        _fixture.UpdateConversationValidatorMock.ValidateAsync(Arg.Any<UpdateConversationDto>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());
        var conversation = Conversation.Create("Initial Name", Guid.NewGuid(), currentUserID).Value;
        
        _fixture.ConversationRepositoryMock.GetConversationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(conversation);
        _fixture.ConversationRepositoryMock
            .UpdateAsync(Arg.Is<Conversation>(c =>
                c.Name == updateDto.Name &&
                c.ProfilePictureID == updateDto.ProfilePictureID &&
                c.CreatedByID == currentUserID), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fixture.UnitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _fixture.ConversationService.UpdateConversationAsync(updateDto, currentUserID);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        await _fixture.ConversationRepositoryMock.Received(1).UpdateAsync(
            Arg.Is<Conversation>(c =>
                c.Name == updateDto.Name &&
                c.ProfilePictureID == updateDto.ProfilePictureID &&
                c.CreatedByID == currentUserID), 
            Arg.Any<CancellationToken>());
        await _fixture.UnitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateConversationAsync_WithNonExistentConversation_ShouldReturnFailResult()
    {
        // Arrange
        var updateDto = new UpdateConversationDto(Guid.NewGuid(), "Updated Conversation", Guid.NewGuid());
        var currentUserID = Guid.NewGuid();
        
        _fixture.UpdateConversationValidatorMock.ValidateAsync(Arg.Any<UpdateConversationDto>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());
        _fixture.ConversationRepositoryMock.GetConversationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);
        _fixture.ConversationRepositoryMock.UpdateAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fixture.UnitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _fixture.ConversationService.UpdateConversationAsync(updateDto, currentUserID);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("Conversation not found or you are not part of it.", result.Errors[0].Message);
    }
    
    [Fact]
    public async Task UpdateConversationAsync_WithInvalidUserId_ShouldReturnFailResult()
    {
        // Arrange
        var updateDto = new UpdateConversationDto(Guid.NewGuid(), "Updated Conversation", Guid.NewGuid());
        var currentUserID = Guid.NewGuid();
        _fixture.UpdateConversationValidatorMock.ValidateAsync(Arg.Any<UpdateConversationDto>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());
        
        var conversation = Conversation.Create("Initial Name", Guid.NewGuid(), Guid.NewGuid()).Value; // Different user ID
        
        _fixture.ConversationRepositoryMock.GetConversationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(conversation);
        
        // Act
        var result = await _fixture.ConversationService.UpdateConversationAsync(updateDto, currentUserID);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("You can only update conversations you created.", result.Errors[0].Message);
    }
}