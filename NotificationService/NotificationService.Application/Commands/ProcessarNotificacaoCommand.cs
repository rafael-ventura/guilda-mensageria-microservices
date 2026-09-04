using MediatR;

namespace NotificationService.Application.Commands;

/// <summary>
/// Command interno para processar o envio de uma notificação
/// </summary>
public record ProcessarNotificacaoCommand(
    Guid RecadoId,
    string Destinatario,
    string Remetente,
    string Tipo,
    string Mensagem
) : IRequest<ProcessarNotificacaoResult>;

public record ProcessarNotificacaoResult(bool Enviada, string? Erro);
