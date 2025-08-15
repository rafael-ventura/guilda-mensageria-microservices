using DispatchService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatchService.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração EF Core para a entidade OutboxMessage
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever(); // Guid gerado no domínio

        builder.Property(o => o.EventType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.EventData)
            .IsRequired()
            .HasColumnType("nvarchar(max)"); // JSON

        builder.Property(o => o.CriadoEm)
            .IsRequired();

        builder.Property(o => o.ProcessadoEm);

        builder.Property(o => o.Processado)
            .IsRequired();

        builder.Property(o => o.TentativasProcessamento)
            .IsRequired();

        builder.Property(o => o.ProximaTentativaEm);

        builder.Property(o => o.ErroUltimoProcessamento)
            .HasMaxLength(2000);

        // Índices para performance
        builder.HasIndex(o => o.Processado);
        builder.HasIndex(o => o.CriadoEm);
        builder.HasIndex(o => o.ProximaTentativaEm);
        builder.HasIndex(o => new { o.Processado, o.ProximaTentativaEm, o.TentativasProcessamento });
    }
}
