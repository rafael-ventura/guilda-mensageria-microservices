using DeliveryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryService.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Entrega
/// </summary>
public class EntregaConfiguration : IEntityTypeConfiguration<Entrega>
{
    public void Configure(EntityTypeBuilder<Entrega> builder)
    {
        builder.ToTable("Entregas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.RecadoId)
            .IsRequired();

        builder.Property(e => e.Destinatario)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.EnderecoEntrega)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Tentativas)
            .IsRequired();

        builder.Property(e => e.CriadoEm)
            .IsRequired();

        builder.Property(e => e.AtualizadoEm);
        builder.Property(e => e.EntregueEm);

        builder.Property(e => e.UltimoErro)
            .HasMaxLength(2000);

        // Idempotência: uma única Entrega por Recado, mesmo com redelivery da mensagem
        builder.HasIndex(e => e.RecadoId).IsUnique();
        builder.HasIndex(e => e.Status);
    }
}
