using GuildaMensageria.Contracts.Events;
using InboxService.Application.Commands;
using InboxService.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InboxService.Integration.EventsIn;

/// <summary>
/// Consumer que materializa a conclusão de uma entrega na timeline do destinatário
/// </summary>
public class EntregaConcluidaEventConsumer : IConsumer<EntregaConcluidaEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<EntregaConcluidaEventConsumer> _logger;

    public EntregaConcluidaEventConsumer(IMediator mediator, ILogger<EntregaConcluidaEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EntregaConcluidaEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation(
            "✅ Recebido EntregaConcluidaEvent - RecadoId: {RecadoId}",
            evento.RecadoId);

        await _mediator.Send(new AtualizarStatusNaTimelineCommand(
            evento.RecadoId,
            evento.Destinatario,
            StatusTimelineRecado.Entregue,
            evento.EntregueEm,
            MotivoFalha: null), context.CancellationToken);
    }
}
