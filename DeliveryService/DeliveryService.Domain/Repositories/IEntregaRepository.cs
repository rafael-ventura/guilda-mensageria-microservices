using DeliveryService.Domain.Entities;

namespace DeliveryService.Domain.Repositories;

/// <summary>
/// Interface do repositório de Entregas
/// </summary>
public interface IEntregaRepository
{
    Task<Entrega?> GetByRecadoIdAsync(Guid recadoId, CancellationToken cancellationToken = default);
    Task AddAsync(Entrega entrega, CancellationToken cancellationToken = default);
    Task UpdateAsync(Entrega entrega, CancellationToken cancellationToken = default);
}
