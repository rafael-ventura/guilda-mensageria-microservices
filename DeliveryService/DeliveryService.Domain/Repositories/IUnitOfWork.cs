namespace DeliveryService.Domain.Repositories;

/// <summary>
/// Interface para Unit of Work - coordena persistência entre repositórios
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IEntregaRepository Entregas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
