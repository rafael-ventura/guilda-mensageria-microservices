using DispatchService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DispatchService.Infrastructure.Data;

/// <summary>
/// DbContext do DispatchService
/// </summary>
public class DispatchDbContext : DbContext
{
    public DbSet<Recado> Recados { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    public DispatchDbContext(DbContextOptions<DispatchDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configurações
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DispatchDbContext).Assembly);
    }
}
