using Application.Contracts;
using Domain.Entities;
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

    public async Task<Comment?> GetCommentAsync(Guid postID, CancellationToken cancellationToken = default)
    {
        return await _db.Comments
            .AsNoTracking()
            .Where(c => c.DeletedAt == null)
            .FirstOrDefaultAsync(c => c.PostID == postID, cancellationToken);
    }

    public async Task<List<Comment>> GetCommentsByPostAsync(Guid postID, CancellationToken cancellationToken = default)
    {
        return await _db.Comments
            .AsNoTracking()
            .Where(c => c.PostID == postID && c.DeletedAt == null)
            .Include(c => c.Author)
            .ToListAsync(cancellationToken);
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
        comment.MarkDeleted();
        _db.Comments.Update(comment);
        return Task.CompletedTask;
    }
}