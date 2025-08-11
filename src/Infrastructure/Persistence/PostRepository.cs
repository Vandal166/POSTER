using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class PostRepository : IPostRepository
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
                    p.CreatedAt,
                    p.VideoFileID,
                    p.Images.Select(pi => pi.ID).ToArray()
                )
            )
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<Post?> GetPostByIDAsync(Guid postID, CancellationToken ct = default)
    {
        return await _db.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments).ThenInclude(c => c.Author)
            .Include(p => p.Images)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.ID == postID, ct);
    }
    
    public async Task<PostDto?> GetPostByCommentAsync(Guid commentID, CancellationToken cancellationToken = default)
    {
        return await _db.Posts
            .Where(p => p.Comments.Any(c => c.ID == commentID))
            .Select(p => new PostDto
                (
                    p.ID,
                    p.Author.Username,
                    p.Author.AvatarPath,
                    p.Content,
                    p.CreatedAt,
                    p.VideoFileID,
                    p.Images.Select(pi => pi.ImageFileID).ToArray()
                )
            )
            .FirstOrDefaultAsync(cancellationToken);
    }

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
    
    // using Keyset pagination
    public async Task<List<PostDto>> GetAllAsync(DateTime? lastCreatedAt, int pageSize, CancellationToken ct = default)
    {
        IQueryable<Post> query = _db.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt);

        if (lastCreatedAt.HasValue)
        {
            var utcCreatedAt = DateTime.SpecifyKind(lastCreatedAt.Value, DateTimeKind.Utc);
            query = query.Where(p => p.CreatedAt < utcCreatedAt);
        }

        return await query
            .Take(pageSize)
            .Select(p => new PostDto(
                p.ID,
                p.Author.Username,
                p.Author.AvatarPath,
                p.Content,
                p.CreatedAt,
                p.VideoFileID,
                p.Images.Select(pi => pi.ImageFileID).ToArray()
            ))
            .ToListAsync(ct);
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
       // await _db.Posts.Where(p => p.ID == post.ID).ExecuteDeleteAsync(cancellationToken);
        
        _db.Posts.Remove(post);
        return Task.CompletedTask;
    }
}