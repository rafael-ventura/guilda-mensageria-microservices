using DispatchService.Domain.Entities;

namespace DispatchService.Domain.Repositories;

/// <summary>
/// Interface do repositório de Recados
/// </summary>
public interface IRecadoRepository
{
    Task<Recado?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recado>> GetByRemetenteAsync(string remetente, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recado>> GetByDestinatarioAsync(string destinatario, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recado>> GetByStatusAsync(StatusRecado status, CancellationToken cancellationToken = default);
    Task AddAsync(Recado recado, CancellationToken cancellationToken = default);
    Task UpdateAsync(Recado recado, CancellationToken cancellationToken = default);
    Task DeleteAsync(Recado recado, CancellationToken cancellationToken = default);
}
