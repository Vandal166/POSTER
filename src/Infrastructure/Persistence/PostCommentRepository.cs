using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PostCommentRepository : IPostCommentRepository
{
    private readonly PosterDbContext _db;
    public PostCommentRepository(PosterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsAsync(Guid commentID, CancellationToken cancellationToken = default)
    {
        return await _db.Comments
            .AsNoTracking()
            .AnyAsync(c => c.ID == commentID, cancellationToken);
    }

    public async Task<CommentDto?> GetCommentAsync(Guid commentID, CancellationToken cancellationToken = default)
    {
        return await _db.Comments
            .AsNoTracking()
            .Where(c => c.ID == commentID)
            .Select(c => new CommentDto(
                c.ID,
                c.Author.Username,
                c.Author.AvatarPath,
                c.Content,
                c.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IPagedList<CommentDto>> GetCommentsByPostAsync(Guid postID, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var commentsResponse =  _db.Comments
            .AsNoTracking()
            .Where(c => c.PostID == postID && c.ParentCommentID == null)
            .OrderBy(p => p.CreatedAt)
            .Select(c => new CommentDto(
                c.ID,
                c.Author.Username,
                c.Author.AvatarPath,
                c.Content,
                c.CreatedAt
            ));
        
        return await PagedList<CommentDto>.CreateAsync(commentsResponse, page, pageSize, cancellationToken);
    }
    
    public async Task<IPagedList<CommentDto>> GetCommentsByCommentAsync(Guid commentID, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var commentsResponse = _db.Comments
            .AsNoTracking()
            .Where(c => c.ParentCommentID == commentID)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(
                c.ID,
                c.Author.Username,
                c.Author.AvatarPath,
                c.Content,
                c.CreatedAt
            ));

        return await PagedList<CommentDto>.CreateAsync(commentsResponse, page, pageSize, cancellationToken);
    }

    public async Task<int> GetCommentsCountByPostAsync(Guid postID, CancellationToken ct = default)
    {
        return await _db.Comments
            .AsNoTracking()
            .CountAsync(c => c.PostID == postID && c.ParentCommentID == null, ct);
    }

    public async Task<int> GetCommentsCountByCommentAsync(Guid commentID, CancellationToken ct = default)
    {
        return await _db.Comments
            .AsNoTracking()
            .CountAsync(c => c.ParentCommentID == commentID, ct);
    }

    public Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _db.Comments.Add(comment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _db.Comments.Remove(comment);
        return Task.CompletedTask;
    }
}