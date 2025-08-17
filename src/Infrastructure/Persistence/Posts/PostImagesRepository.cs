using Application.Contracts.Persistence;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Persistence;

internal sealed class PostImagesRepository : IPostImageRepository
{
    private readonly PosterDbContext _db;
    public PostImagesRepository(PosterDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(PostImage postImage, CancellationToken cancellationToken = default)
    {
        _db.PostImages.Add(postImage);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<PostImage> postImages, CancellationToken cancellationToken = default)
    {
        _db.PostImages.AddRange(postImages);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PostImage postImage, CancellationToken cancellationToken = default)
    {
        _db.PostImages.Remove(postImage);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<PostImage> postImages, CancellationToken cancellationToken = default)
    {
        _db.PostImages.RemoveRange(postImages);
        return Task.CompletedTask;
    }
}