using Application.DTOs;
using Domain.Entities;
using FluentResults;

namespace Application.Contracts;

public interface IPostService
{
    Task<Result<Guid>> CreatePostAsync(CreatePostDto dto, Guid userID, CancellationToken cancellationToken);
    IAsyncEnumerable<Post> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Post?> DeletePostAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Result> LikePostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
    Task<Result> UnlikePostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
}