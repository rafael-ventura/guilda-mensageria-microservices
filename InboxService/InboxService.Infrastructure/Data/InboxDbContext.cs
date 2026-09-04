using InboxService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InboxService.Infrastructure.Data;

/// <summary>
/// DbContext do InboxService
/// </summary>
public class InboxDbContext : DbContext
{
    public DbSet<ItemTimeline> ItensTimeline { get; set; } = null!;

    public InboxDbContext(DbContextOptions<InboxDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboxDbContext).Assembly);
    }
}
