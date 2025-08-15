using DispatchService.Domain.Repositories;
using DispatchService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace DispatchService.Infrastructure.Repositories;

/// <summary>
/// Implementação do Unit of Work usando EF Core
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DispatchDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    public IRecadoRepository Recados { get; }
    public IOutboxRepository OutboxMessages { get; }

    public UnitOfWork(DispatchDbContext context)
    {
        _context = context;
        Recados = new RecadoRepository(context);
        OutboxMessages = new OutboxRepository(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Uma transação já está ativa");

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Nenhuma transação ativa para commit");

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Nenhuma transação ativa para rollback");

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }
}
