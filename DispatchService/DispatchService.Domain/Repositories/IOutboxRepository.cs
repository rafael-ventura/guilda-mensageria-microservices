using DispatchService.Domain.Entities;

namespace DispatchService.Domain.Repositories;

/// <summary>
/// Interface do repositório de OutboxMessages
/// </summary>
public interface IOutboxRepository
{
    Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OutboxMessage>> GetPendentesAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default);
    Task UpdateAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default);
    Task DeleteAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default);
}
