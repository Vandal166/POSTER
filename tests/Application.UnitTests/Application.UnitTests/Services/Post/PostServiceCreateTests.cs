using Application.DTOs;

namespace Application.UnitTests.Services;

public class PostServiceTests
{
    private readonly PostServiceFixture _fixture = new();

    [Fact]
    public async Task CreatePostAsync_WithValidData_ShouldReturnPostId()
    {
        // Arrange
        var createPostDto = new CreatePostDto
        (
            "This is a valid post content",
            Guid.NewGuid(),
            new[] { Guid.NewGuid(), Guid.NewGuid() }
        );
        var userId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
       
        _fixture.CreatePostValidatorMock.ValidateAsync(Arg.Any<CreatePostDto>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());
        
        _ = Post.Create(userId, createPostDto.Content, createPostDto.VideoFileID).Value;
        
        _fixture.PostRepositoryMock.AddAsync(Arg.Is<Post>(x => x.Content == createPostDto.Content && x.AuthorID == userId), 
            Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        
        _fixture.PostImageRepositoryMock.AddRangeAsync(Arg.Is<IEnumerable<PostImage>>(x => x.Count() == createPostDto.ImageFileIDs.Length), 
            Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _fixture.UowMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        
        // Act
        var result = await _fixture.PostService.CreatePostAsync(createPostDto, userId, cancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        await _fixture.PostRepositoryMock.Received(1)
            .AddAsync(Arg.Is<Post>(x => 
                x.Content == createPostDto.Content && 
                x.AuthorID == userId), 
            Arg.Any<CancellationToken>());
        await _fixture.PostImageRepositoryMock.Received(1)
            .AddRangeAsync(Arg.Is<IEnumerable<PostImage>>(x => x.Count() == createPostDto.ImageFileIDs.Length), 
            Arg.Any<CancellationToken>());
        await _fixture.UowMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePostAsync_WithEmptyContent_ShouldReturnValidationError()
    {
        // Arrange
        _fixture.CreatePostValidatorMock.ValidateAsync(Arg.Any<CreatePostDto>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult(new[] {
                new FluentValidation.Results.ValidationFailure("Content", "Required")
            }));
       
        var createPostDto = new CreatePostDto
        (
            "",
             null,
            null
        );
        var userId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        // Act
        var result = await _fixture.PostService.CreatePostAsync(createPostDto, userId, cancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Required");
        
        // ensuring that AddAsync, AddRangeAsync, and SaveChangesAsync were never called due to validation failure
        await _fixture.PostRepositoryMock.Received(0)
            .AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
       await _fixture.PostImageRepositoryMock.Received(0)
            .AddRangeAsync(Arg.Any<IEnumerable<PostImage>>(), Arg.Any<CancellationToken>());
        await _fixture.UowMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}