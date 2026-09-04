using InboxService.Domain.Entities;
using InboxService.Domain.Repositories;
using InboxService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InboxService.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Itens da Timeline usando EF Core
/// </summary>
public class ItemTimelineRepository : IItemTimelineRepository
{
    private readonly InboxDbContext _context;

    public ItemTimelineRepository(InboxDbContext context)
    {
        _context = context;
    }

    public async Task<ItemTimeline?> GetByRecadoIdAsync(Guid recadoId, CancellationToken cancellationToken = default)
    {
        return await _context.ItensTimeline
            .FirstOrDefaultAsync(i => i.RecadoId == recadoId, cancellationToken);
    }

    public async Task<IEnumerable<ItemTimeline>> GetByDestinatarioAsync(string destinatario, CancellationToken cancellationToken = default)
    {
        return await _context.ItensTimeline
            .Where(i => i.Destinatario == destinatario)
            .OrderByDescending(i => i.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ItemTimeline item, CancellationToken cancellationToken = default)
    {
        await _context.ItensTimeline.AddAsync(item, cancellationToken);
    }

    public Task UpdateAsync(ItemTimeline item, CancellationToken cancellationToken = default)
    {
        _context.ItensTimeline.Update(item);
        return Task.CompletedTask;
    }
}
