using Domain.Entities;

namespace Application.Contracts.Persistence;

public interface IPostImageRepository
{
    Task AddAsync(PostImage postImage, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<PostImage> postImages, CancellationToken cancellationToken = default);
    Task DeleteAsync(PostImage postImage, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<PostImage> postImages, CancellationToken cancellationToken = default);
}