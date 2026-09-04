using InboxService.Domain.Repositories;
using InboxService.Infrastructure.Data;

namespace InboxService.Infrastructure.Repositories;

/// <summary>
/// Implementação do Unit of Work usando EF Core
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly InboxDbContext _context;
    private bool _disposed;

    public IItemTimelineRepository ItensTimeline { get; }

    public UnitOfWork(InboxDbContext context)
    {
        _context = context;
        ItensTimeline = new ItemTimelineRepository(context);
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
