using InboxService.Domain.Entities;

namespace InboxService.Domain.Repositories;

/// <summary>
/// Interface do repositório de Itens da Timeline
/// </summary>
public interface IItemTimelineRepository
{
    Task<ItemTimeline?> GetByRecadoIdAsync(Guid recadoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ItemTimeline>> GetByDestinatarioAsync(string destinatario, CancellationToken cancellationToken = default);
    Task AddAsync(ItemTimeline item, CancellationToken cancellationToken = default);
    Task UpdateAsync(ItemTimeline item, CancellationToken cancellationToken = default);
}
