using DispatchService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatchService.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Recado
/// </summary>
public class RecadoConfiguration : IEntityTypeConfiguration<Recado>
{
    public void Configure(EntityTypeBuilder<Recado> builder)
    {
        builder.ToTable("Recados");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever(); // Guid gerado no domínio

        builder.Property(r => r.Remetente)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Destinatario)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Conteudo)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.EnderecoEntrega)
            .HasMaxLength(500);

        builder.Property(r => r.CriadoEm)
            .IsRequired();

        builder.Property(r => r.AtualizadoEm);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>(); // Enum como int no banco

        // Índices
        builder.HasIndex(r => r.Remetente);
        builder.HasIndex(r => r.Destinatario);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CriadoEm);
    }
}
