using GuildaMensageria.Contracts.Events;
using InboxService.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InboxService.Integration.EventsIn;

/// <summary>
/// Consumer que materializa a criação de um recado na timeline do destinatário
/// </summary>
public class RecadoCriadoEventConsumer : IConsumer<RecadoCriadoEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecadoCriadoEventConsumer> _logger;

    public RecadoCriadoEventConsumer(IMediator mediator, ILogger<RecadoCriadoEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RecadoCriadoEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation(
            "📦 Recebido RecadoCriadoEvent - RecadoId: {RecadoId}, Destinatario: {Destinatario}",
            evento.RecadoId, evento.Destinatario);

        await _mediator.Send(new RegistrarCriacaoNaTimelineCommand(
            evento.RecadoId,
            evento.Remetente,
            evento.Destinatario,
            evento.Conteudo,
            evento.EnderecoEntrega,
            evento.CriadoEm), context.CancellationToken);
    }
}
