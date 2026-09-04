using GuildaMensageria.Contracts.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Commands;

namespace NotificationService.Integration.CommandsIn;

/// <summary>
/// Consumer que processa comandos de envio de notificação
/// </summary>
public class EnviarNotificacaoCommandConsumer : IConsumer<EnviarNotificacaoCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<EnviarNotificacaoCommandConsumer> _logger;

    public EnviarNotificacaoCommandConsumer(IMediator mediator, ILogger<EnviarNotificacaoCommandConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EnviarNotificacaoCommand> context)
    {
        var comando = context.Message;

        _logger.LogInformation(
            "🔔 Recebido comando EnviarNotificacao - RecadoId: {RecadoId}, Tipo: {Tipo}, Destinatario: {Destinatario}",
            comando.RecadoId, comando.Tipo, comando.Destinatario);

        var resultado = await _mediator.Send(new ProcessarNotificacaoCommand(
            comando.RecadoId,
            comando.Destinatario,
            comando.Remetente,
            comando.Tipo.ToString(),
            comando.Mensagem), context.CancellationToken);

        if (!resultado.Enviada)
        {
            // Relança para o retry do MassTransit tentar de novo
            throw new InvalidOperationException(resultado.Erro ?? "Falha ao enviar notificação");
        }
    }
}
