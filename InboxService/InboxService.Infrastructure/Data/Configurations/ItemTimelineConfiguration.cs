using InboxService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InboxService.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração EF Core para a entidade ItemTimeline
/// </summary>
public class ItemTimelineConfiguration : IEntityTypeConfiguration<ItemTimeline>
{
    public void Configure(EntityTypeBuilder<ItemTimeline> builder)
    {
        builder.ToTable("ItensTimeline");

        // RecadoId é a chave natural: uma linha por recado, atualizada in-place
        builder.HasKey(i => i.RecadoId);

        builder.Property(i => i.RecadoId)
            .ValueGeneratedNever();

        builder.Property(i => i.Remetente)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Destinatario)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Conteudo)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.EnderecoEntrega)
            .HasMaxLength(500);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.CriadoEm)
            .IsRequired();

        builder.Property(i => i.AtualizadoEm);
        builder.Property(i => i.EntregueEm);

        builder.Property(i => i.MotivoFalha)
            .HasMaxLength(2000);

        builder.HasIndex(i => i.Destinatario);
        builder.HasIndex(i => i.Status);
    }
}
