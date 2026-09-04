namespace InboxService.Domain.Repositories;

/// <summary>
/// Interface para Unit of Work - coordena persistência entre repositórios
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IItemTimelineRepository ItensTimeline { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
