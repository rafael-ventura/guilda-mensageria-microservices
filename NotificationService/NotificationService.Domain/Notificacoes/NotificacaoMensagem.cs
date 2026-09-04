namespace NotificationService.Domain.Notificacoes;

/// <summary>
/// Value object com os dados de uma notificação a ser enviada
/// </summary>
public record NotificacaoMensagem(
    Guid RecadoId,
    string Destinatario,
    string Remetente,
    string Tipo,
    string Mensagem
);
