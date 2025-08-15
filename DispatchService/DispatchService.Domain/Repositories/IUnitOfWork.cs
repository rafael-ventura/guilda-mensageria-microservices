namespace DispatchService.Domain.Repositories;

/// <summary>
/// Interface para Unit of Work - coordena transações entre repositórios
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRecadoRepository Recados { get; }
    IOutboxRepository OutboxMessages { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
