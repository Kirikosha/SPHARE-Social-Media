using System.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        var efTransaction = await _context.Database.BeginTransactionAsync(ct);
        _transaction = efTransaction.GetDbTransaction();
        return _transaction;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct)
    {
        if (HasActiveTransaction)
            await _context.Database.CommitTransactionAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct)
    {
        if (HasActiveTransaction)
            await _context.Database.RollbackTransactionAsync(ct);
    }

    public bool HasActiveTransaction => _context.Database.CurrentTransaction != null;
}