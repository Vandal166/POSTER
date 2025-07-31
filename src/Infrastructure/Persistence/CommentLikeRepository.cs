using Application.Contracts.Persistence;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class CommentLikeRepository : ICommentLikeRepository
{
    private readonly PosterDbContext _db;
    public CommentLikeRepository(PosterDbContext db)
    {
        _db = db;
    }

    public async Task<CommentLike?> GetLikeAsync(Guid commentID, Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.CommentLikes
            .AsNoTracking()
            .FirstOrDefaultAsync(cl => cl.CommentID == commentID && cl.UserID == userID, cancellationToken);
    }

    public async Task<bool> IsCommentLikedByUserAsync(Guid commentID, Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.CommentLikes
            .AsNoTracking()
            .AnyAsync(cl => cl.CommentID == commentID && cl.UserID == userID, cancellationToken);
    }

    public Task AddLikeAsync(CommentLike like, CancellationToken cancellationToken = default)
    {
        _db.CommentLikes.Add(like);
        return Task.CompletedTask;
    }

    public Task RemoveLikeAsync(CommentLike like, CancellationToken cancellationToken = default)
    {
        _db.CommentLikes.Remove(like);
        return Task.CompletedTask;
    }

    public async Task<List<CommentLike>> GetLikesByCommentAsync(Guid commentID, CancellationToken cancellationToken = default)
    {
        return await _db.CommentLikes
            .AsNoTracking()
            .Where(cl => cl.CommentID == commentID)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetLikesCountByCommentAsync(Guid postID, CancellationToken cancellationToken = default)
    {
        return await _db.CommentLikes
            .AsNoTracking()
            .CountAsync(cl => cl.CommentID == postID, cancellationToken);
    }
}