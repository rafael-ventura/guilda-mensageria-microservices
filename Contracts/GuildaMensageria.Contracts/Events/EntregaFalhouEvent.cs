namespace GuildaMensageria.Contracts.Events;

public record EntregaFalhouEvent
{
    public Guid RecadoId { get; init; }
    public string Destinatario { get; init; } = string.Empty;
    public DateTime FalhouEm { get; init; }
    public string MotivoFalha { get; init; } = string.Empty;
    public int TentativasRealizadas { get; init; }
    public bool DeveReitentar { get; init; }
}
