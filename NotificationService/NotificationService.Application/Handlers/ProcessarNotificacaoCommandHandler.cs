using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Commands;
using NotificationService.Domain.Notificacoes;

namespace NotificationService.Application.Handlers;

/// <summary>
/// Orquestra o envio de uma notificação (Template Method): monta a mensagem, delega
/// o envio de fato ao canal configurado (Strategy) e trata sucesso/erro de forma
/// uniforme, independente de qual canal está por trás.
/// </summary>
public class ProcessarNotificacaoCommandHandler : IRequestHandler<ProcessarNotificacaoCommand, ProcessarNotificacaoResult>
{
    private readonly ICanalNotificacao _canal;
    private readonly ILogger<ProcessarNotificacaoCommandHandler> _logger;

    public ProcessarNotificacaoCommandHandler(ICanalNotificacao canal, ILogger<ProcessarNotificacaoCommandHandler> logger)
    {
        _canal = canal;
        _logger = logger;
    }

    public async Task<ProcessarNotificacaoResult> Handle(ProcessarNotificacaoCommand request, CancellationToken cancellationToken)
    {
        var mensagem = new NotificacaoMensagem(
            request.RecadoId,
            request.Destinatario,
            request.Remetente,
            request.Tipo,
            request.Mensagem);

        try
        {
            await _canal.EnviarAsync(mensagem, cancellationToken);
            return new ProcessarNotificacaoResult(true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Falha ao enviar notificação via {Canal} - RecadoId: {RecadoId}",
                _canal.Nome, request.RecadoId);

            return new ProcessarNotificacaoResult(false, ex.Message);
        }
    }
}
