using System.Runtime.CompilerServices;
using Application.Contracts;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PostRepository : IPostRepository
{
    private readonly PosterDbContext _db;
    public PostRepository(PosterDbContext db)
    {
        _db = db;
    }
    
    private static readonly Func<PosterDbContext, IAsyncEnumerable<Post>> _getPostsQuery = EF.CompileAsyncQuery(
        (PosterDbContext db) => db.Posts
            .AsNoTracking()
            .Where(p => p.DeletedAt == null)
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author));


    public Task<bool> ExistsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return _db.Posts
            .AsNoTracking()
            .AnyAsync(p => p.ID == postId && p.DeletedAt == null, cancellationToken);
    }

    public async Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(p => p.DeletedAt == null)
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(p => p.ID == id, cancellationToken);
    }

    public async Task<List<Post>> GetUserFeedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(p => p.DeletedAt == null && p.AuthorID == userId)
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author)
            .ToListAsync(cancellationToken);
    }
//TODO pagination
//TODO change to List<Post> with pagination
    public async IAsyncEnumerable<Post> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var post in _getPostsQuery(_db).WithCancellation(cancellationToken))
        {
            yield return post;
        }
    }

    public Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        _db.Posts.Add(post);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Post post, CancellationToken cancellationToken = default)
    {
        _db.Posts.Update(post);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Post post, CancellationToken cancellationToken = default)
    {
        post.MarkDeleted();
        _db.Posts.Update(post);
        return Task.CompletedTask;
    }
}