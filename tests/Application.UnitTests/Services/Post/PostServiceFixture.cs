using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.Services;
using FluentValidation;

namespace Application.UnitTests.Services;

public class PostServiceFixture : IDisposable
{
   public IPostRepository PostRepositoryMock { get; }
   public IPostImageRepository PostImageRepositoryMock { get; }
   public IValidator<CreatePostDto> CreatePostValidatorMock { get; }
   public IUnitOfWork UowMock { get; }
   internal PostService PostService { get; }

    public PostServiceFixture()
    {
        PostRepositoryMock = Substitute.For<IPostRepository>();
        PostImageRepositoryMock = Substitute.For<IPostImageRepository>();
        CreatePostValidatorMock = Substitute.For<IValidator<CreatePostDto>>();
        UowMock = Substitute.For<IUnitOfWork>();

        PostService = new PostService(
            PostRepositoryMock,
            PostImageRepositoryMock,
            CreatePostValidatorMock,
            UowMock
        );
    }
    
    public void Reset()
    {
        // Reset all mocks to their initial state
        PostRepositoryMock.ClearReceivedCalls();
        PostImageRepositoryMock.ClearReceivedCalls();
        CreatePostValidatorMock.ClearReceivedCalls();
        UowMock.ClearReceivedCalls();
    }
    public void Dispose() 
    {
        Reset();
    }
}