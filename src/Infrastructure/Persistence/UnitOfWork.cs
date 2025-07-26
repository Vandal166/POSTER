using Application.Contracts;
using Infrastructure.Data;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly PosterDbContext _db;
    public UnitOfWork(PosterDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
    
    public Task BeginTransactionAsync()
    {
        if (_db.Database.CurrentTransaction == null)
            return _db.Database.BeginTransactionAsync();
        
        return Task.CompletedTask;
    }
    
    public Task CommitTransactionAsync()
    {
        if (_db.Database.CurrentTransaction != null)
            return _db.Database.CommitTransactionAsync();
        
        throw new InvalidOperationException("No transaction to commit.");
    }

    public Task RollbackTransactionAsync()
    {
        if (_db.Database.CurrentTransaction != null)
            return _db.Database.RollbackTransactionAsync();
            
        throw new InvalidOperationException("No transaction to rollback.");
    }
}