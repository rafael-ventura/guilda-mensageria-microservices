using GuildaMensageria.Contracts.Events;
using InboxService.Application.Commands;
using InboxService.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InboxService.Integration.EventsIn;

/// <summary>
/// Consumer que materializa a falha definitiva de uma entrega na timeline do destinatário
/// </summary>
public class EntregaFalhouEventConsumer : IConsumer<EntregaFalhouEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<EntregaFalhouEventConsumer> _logger;

    public EntregaFalhouEventConsumer(IMediator mediator, ILogger<EntregaFalhouEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EntregaFalhouEvent> context)
    {
        var evento = context.Message;

        _logger.LogWarning(
            "❌ Recebido EntregaFalhouEvent - RecadoId: {RecadoId}, Motivo: {Motivo}",
            evento.RecadoId, evento.MotivoFalha);

        await _mediator.Send(new AtualizarStatusNaTimelineCommand(
            evento.RecadoId,
            evento.Destinatario,
            StatusTimelineRecado.Falhou,
            evento.FalhouEm,
            evento.MotivoFalha), context.CancellationToken);
    }
}
