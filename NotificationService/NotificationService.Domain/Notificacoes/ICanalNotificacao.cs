namespace NotificationService.Domain.Notificacoes;

/// <summary>
/// Porta (Strategy) para um canal de envio de notificação. Cada implementação em
/// Infrastructure é uma estratégia concreta (Console hoje; Email/SMS/Push amanhã,
/// sem tocar no Application).
/// </summary>
public interface ICanalNotificacao
{
    /// <summary>
    /// Nome do canal, usado em logs e (futuramente) para seleção de estratégia
    /// </summary>
    string Nome { get; }

    Task EnviarAsync(NotificacaoMensagem mensagem, CancellationToken cancellationToken = default);
}
