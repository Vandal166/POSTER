using Application.DTOs;
using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.Services;

public class ConversationCreateTests
{
    private readonly ConversationServiceFixture _fixture = new();
    
    [Fact]
    public async Task CreateConversationAsync_WithValidData_ShouldReturnConversationId()
    {
        // Arrange
        var createDto = new CreateConversationDto
        (
            "Test Conversation",
            Guid.NewGuid()
        );
        
        var participantIDs = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var currentUserID = Guid.NewGuid();
        _fixture.CreateConversationValidatorMock.ValidateAsync(Arg.Any<CreateConversationDto>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());
        
        _ = Conversation.Create(createDto.Name, createDto.ProfilePictureFileID, currentUserID).Value;
        
        _fixture.ConversationRepositoryMock.AddAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fixture.UnitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _fixture.ConversationService.CreateConversationAsync(currentUserID, participantIDs, createDto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }
    
    [Fact]
    public async Task CreateConversationAsync_WithInvalidParticipants_ShouldReturnFailResult()
    {
        // Arrange
        var createDto = new CreateConversationDto("Test Conversation", Guid.NewGuid());
        var participantIDs = new List<Guid> { Guid.NewGuid() }; // Less than 2 participants
        var currentUserID = Guid.NewGuid();
    
        // Act
        var result = await _fixture.ConversationService.CreateConversationAsync(currentUserID, participantIDs, createDto);
    
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("At least two participants are required to create a conversation.", result.Errors[0].Message);
    }
    
    [Fact]
    public async Task CreateConversationAsync_WithInvalidDto_ShouldReturnValidationErrors()
    {
        // Arrange
        var createDto = new CreateConversationDto("", Guid.Empty);
        var participantIDs = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }; // Valid participant count
        var currentUserID = Guid.NewGuid();
    
        _fixture.CreateConversationValidatorMock.ValidateAsync(Arg.Any<CreateConversationDto>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Name", "Conversation name cannot be empty."),
                new FluentValidation.Results.ValidationFailure("ProfilePictureFileID", "Profile picture path cannot be empty.")
            }));
    
        // Act
        var result = await _fixture.ConversationService.CreateConversationAsync(currentUserID, participantIDs, createDto);
    
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Message == "Conversation name cannot be empty.");
        Assert.Contains(result.Errors, e => e.Message == "Profile picture path cannot be empty.");
    }
}