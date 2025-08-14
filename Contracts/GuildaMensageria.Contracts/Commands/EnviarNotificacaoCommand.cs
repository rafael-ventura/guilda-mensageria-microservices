namespace GuildaMensageria.Contracts.Commands;

public record EnviarNotificacaoCommand
{
    public Guid RecadoId { get; init; }
    public string Destinatario { get; init; } = string.Empty;
    public string Remetente { get; init; } = string.Empty;
    public TipoNotificacao Tipo { get; init; }
    public string Mensagem { get; init; } = string.Empty;
}

public enum TipoNotificacao
{
    RecadoCriado,
    EntregaConcluida,
    EntregaFalhou
}
