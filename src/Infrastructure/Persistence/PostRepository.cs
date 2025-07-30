using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Common;
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

    public async Task<bool> ExistsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _db.Posts
            .AsNoTracking()
            .AnyAsync(p => p.ID == postId, cancellationToken);
    }

    /*public async Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(p => p.ID == id, cancellationToken);
    }*/
    
    public async Task<PostDto?> GetPostAsync(Guid postID, CancellationToken ct = default)
    {
        return await _db.Posts
            .Where(p => p.ID == postID)
            .Select
            (p => new PostDto
                (
                    p.ID,
                    p.Author.Username,
                    p.Author.AvatarPath,
                    p.Content,
                    p.CreatedAt
                )
            )
            .FirstOrDefaultAsync(ct);
    }
    /*,
                    p.Likes.Count,
                    p.Comments.Count,
                    p.Views.Count*/

    public async Task<List<Post>> GetUserFeedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(p => p.AuthorID == userId)
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author)
            .ToListAsync(cancellationToken);
    }

    public async Task<IPagedList<PostDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var postsResponse = _db.Posts
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .Select(p => new PostDto(
                p.ID,
                p.Author.Username,
                p.Author.AvatarPath,
                p.Content,
                p.CreatedAt
            ));
        
       return await PagedList<PostDto>.CreateAsync(postsResponse, page, pageSize, cancellationToken);
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
        _db.Posts.Remove(post);
        return Task.CompletedTask;
    }
}