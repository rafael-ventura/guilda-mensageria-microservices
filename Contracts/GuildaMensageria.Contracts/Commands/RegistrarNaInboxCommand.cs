namespace GuildaMensageria.Contracts.Commands;

public record RegistrarNaInboxCommand
{
    public Guid RecadoId { get; init; }
    public string Destinatario { get; init; } = string.Empty;
    public TipoEventoInbox TipoEvento { get; init; }
    public string Conteudo { get; init; } = string.Empty;
    public DateTime OcorreuEm { get; init; }
    public Dictionary<string, object>? DadosAdicionais { get; init; }
}

public enum TipoEventoInbox
{
    RecadoRecebido,
    EntregaIniciada,
    EntregaConcluida,
    EntregaFalhou
}
