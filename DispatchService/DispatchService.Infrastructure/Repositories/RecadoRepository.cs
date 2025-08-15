using DispatchService.Domain.Entities;
using DispatchService.Domain.Repositories;
using DispatchService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DispatchService.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Recados usando EF Core
/// </summary>
public class RecadoRepository : IRecadoRepository
{
    private readonly DispatchDbContext _context;

    public RecadoRepository(DispatchDbContext context)
    {
        _context = context;
    }

    public async Task<Recado?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Recados
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Recado>> GetByRemetenteAsync(string remetente, CancellationToken cancellationToken = default)
    {
        return await _context.Recados
            .Where(r => r.Remetente == remetente)
            .OrderByDescending(r => r.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Recado>> GetByDestinatarioAsync(string destinatario, CancellationToken cancellationToken = default)
    {
        return await _context.Recados
            .Where(r => r.Destinatario == destinatario)
            .OrderByDescending(r => r.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Recado>> GetByStatusAsync(StatusRecado status, CancellationToken cancellationToken = default)
    {
        return await _context.Recados
            .Where(r => r.Status == status)
            .OrderBy(r => r.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Recado recado, CancellationToken cancellationToken = default)
    {
        await _context.Recados.AddAsync(recado, cancellationToken);
    }

    public Task UpdateAsync(Recado recado, CancellationToken cancellationToken = default)
    {
        _context.Recados.Update(recado);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Recado recado, CancellationToken cancellationToken = default)
    {
        _context.Recados.Remove(recado);
        return Task.CompletedTask;
    }
}
