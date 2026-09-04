namespace InboxService.Domain.Entities;

/// <summary>
/// Materialized view da timeline do destinatário — uma linha por recado, atualizada
/// conforme os eventos de criação e de entrega chegam (CQRS, lado de leitura).
/// </summary>
public class ItemTimeline
{
    public Guid RecadoId { get; private set; }
    public string Remetente { get; private set; } = string.Empty;
    public string Destinatario { get; private set; } = string.Empty;
    public string Conteudo { get; private set; } = string.Empty;
    public string? EnderecoEntrega { get; private set; }
    public StatusTimelineRecado Status { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public DateTime? EntregueEm { get; private set; }
    public string? MotivoFalha { get; private set; }

    // EF Core constructor
    private ItemTimeline() { }

    public ItemTimeline(Guid recadoId, string remetente, string destinatario, string conteudo, string? enderecoEntrega, DateTime criadoEm)
    {
        if (recadoId == Guid.Empty)
            throw new ArgumentException("RecadoId é obrigatório", nameof(recadoId));

        RecadoId = recadoId;
        Remetente = remetente.Trim();
        Destinatario = destinatario.Trim();
        Conteudo = conteudo.Trim();
        EnderecoEntrega = enderecoEntrega?.Trim();
        Status = StatusTimelineRecado.Criado;
        CriadoEm = criadoEm;
    }

    /// <summary>
    /// Usado quando um evento de entrega chega antes do RecadoCriadoEvent (sem garantia
    /// de ordem entre exchanges independentes) — cria um registro provisório.
    /// </summary>
    public static ItemTimeline CriarProvisorio(Guid recadoId, string destinatario)
        => new(recadoId, remetente: string.Empty, destinatario, conteudo: string.Empty, enderecoEntrega: null, DateTime.UtcNow);

    public void MarcarComoEntregue(DateTime entregueEm)
    {
        Status = StatusTimelineRecado.Entregue;
        EntregueEm = entregueEm;
        AtualizadoEm = DateTime.UtcNow;
        MotivoFalha = null;
    }

    public void MarcarComoFalhou(string motivo)
    {
        Status = StatusTimelineRecado.Falhou;
        MotivoFalha = motivo;
        AtualizadoEm = DateTime.UtcNow;
    }
}

/// <summary>
/// Status possíveis de um recado na timeline do destinatário
/// </summary>
public enum StatusTimelineRecado
{
    Criado = 1,
    Entregue = 2,
    Falhou = 3
}
