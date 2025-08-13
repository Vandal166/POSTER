using Application.DTOs;
using Domain.Entities;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IPostService
{
    Task<Result<Guid>> CreatePostAsync(CreatePostDto dto, Guid userID, CancellationToken cancellationToken);

    Task<IPagedList<PostDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeletePostAsync(Guid id, Guid currentUserID, CancellationToken cancellationToken = default);
}