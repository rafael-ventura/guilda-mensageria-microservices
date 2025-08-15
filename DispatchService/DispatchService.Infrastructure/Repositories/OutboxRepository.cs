using DispatchService.Domain.Entities;
using DispatchService.Domain.Repositories;
using DispatchService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DispatchService.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de OutboxMessages usando EF Core
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly DispatchDbContext _context;

    public OutboxRepository(DispatchDbContext context)
    {
        _context = context;
    }

    public async Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendentesAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .Where(o => !o.Processado && 
                       (o.ProximaTentativaEm == null || o.ProximaTentativaEm <= DateTime.UtcNow) &&
                       o.TentativasProcessamento < 5)
            .OrderBy(o => o.CriadoEm)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
    {
        await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
    }

    public Task UpdateAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
    {
        _context.OutboxMessages.Update(outboxMessage);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
    {
        _context.OutboxMessages.Remove(outboxMessage);
        return Task.CompletedTask;
    }
}
