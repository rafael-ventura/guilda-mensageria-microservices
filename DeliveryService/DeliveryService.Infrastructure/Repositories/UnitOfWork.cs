using DeliveryService.Domain.Repositories;
using DeliveryService.Infrastructure.Data;

namespace DeliveryService.Infrastructure.Repositories;

/// <summary>
/// Implementação do Unit of Work usando EF Core
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DeliveryDbContext _context;
    private bool _disposed;

    public IEntregaRepository Entregas { get; }

    public UnitOfWork(DeliveryDbContext context)
    {
        _context = context;
        Entregas = new EntregaRepository(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
    }
}
