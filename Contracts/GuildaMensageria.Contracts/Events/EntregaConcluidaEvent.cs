namespace GuildaMensageria.Contracts.Events;

public record EntregaConcluidaEvent
{
    public Guid RecadoId { get; init; }
    public string Destinatario { get; init; } = string.Empty;
    public DateTime EntregueEm { get; init; }
    public string? Observacoes { get; init; }
}
