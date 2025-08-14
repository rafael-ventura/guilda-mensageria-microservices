namespace GuildaMensageria.Contracts.Events;

public record RecadoCriadoEvent
{
    public Guid RecadoId { get; init; }
    public string Remetente { get; init; } = string.Empty;
    public string Destinatario { get; init; } = string.Empty;
    public string Conteudo { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; }
    public string? EnderecoEntrega { get; init; }
}
