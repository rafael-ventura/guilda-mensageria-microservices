using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Repositories;
using DeliveryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Entregas usando EF Core
/// </summary>
public class EntregaRepository : IEntregaRepository
{
    private readonly DeliveryDbContext _context;

    public EntregaRepository(DeliveryDbContext context)
    {
        _context = context;
    }

    public async Task<Entrega?> GetByRecadoIdAsync(Guid recadoId, CancellationToken cancellationToken = default)
    {
        return await _context.Entregas
            .FirstOrDefaultAsync(e => e.RecadoId == recadoId, cancellationToken);
    }

    public async Task AddAsync(Entrega entrega, CancellationToken cancellationToken = default)
    {
        await _context.Entregas.AddAsync(entrega, cancellationToken);
    }

    public Task UpdateAsync(Entrega entrega, CancellationToken cancellationToken = default)
    {
        _context.Entregas.Update(entrega);
        return Task.CompletedTask;
    }
}
