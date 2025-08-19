using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.Services;
using FluentValidation;
using NSubstitute;

namespace Application.UnitTests.Services;

public class ConversationServiceFixture : IDisposable
{
    public IConversationRepository ConversationRepositoryMock { get; }
    public IValidator<CreateConversationDto> CreateConversationValidatorMock { get; }
    public IValidator<UpdateConversationDto> UpdateConversationValidatorMock { get; }
    public IUnitOfWork UnitOfWorkMock { get; }
    public IBlobService BlobServiceMock { get; }
    
    internal ConversationService ConversationService { get; }
    
    public ConversationServiceFixture()
    {
        // Initialize all mocks
        ConversationRepositoryMock = Substitute.For<IConversationRepository>();
        CreateConversationValidatorMock = Substitute.For<IValidator<CreateConversationDto>>();
        UpdateConversationValidatorMock = Substitute.For<IValidator<UpdateConversationDto>>();
        UnitOfWorkMock = Substitute.For<IUnitOfWork>();
        BlobServiceMock = Substitute.For<IBlobService>();
        
        // Create the service
        ConversationService = new ConversationService(
            ConversationRepositoryMock,
            CreateConversationValidatorMock,
            UpdateConversationValidatorMock,
            UnitOfWorkMock,
            BlobServiceMock
        );
    }
    
    public void Reset()
    {
        // Reset all mocks to their initial state
        ConversationRepositoryMock.ClearReceivedCalls();
        CreateConversationValidatorMock.ClearReceivedCalls();
        UpdateConversationValidatorMock.ClearReceivedCalls();
        UnitOfWorkMock.ClearReceivedCalls();
        BlobServiceMock.ClearReceivedCalls();
    }
    public void Dispose() 
    {
        Reset();
    }
}