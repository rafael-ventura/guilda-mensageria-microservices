using DeliveryService.Application.Commands;
using GuildaMensageria.Contracts.Commands;
using GuildaMensageria.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DeliveryService.Integration.EventsIn;

/// <summary>
/// Consumer que processa eventos de recado criado e conduz o processo de entrega.
/// Falha temporária relança a exceção para acionar o retry/circuit breaker do
/// MassTransit; falha definitiva (tentativas esgotadas) publica o evento de falha
/// e não relança, encerrando o reprocessamento da mensagem.
/// </summary>
public class RecadoCriadoEventConsumer : IConsumer<RecadoCriadoEvent>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RecadoCriadoEventConsumer> _logger;

    public RecadoCriadoEventConsumer(IMediator mediator, IPublishEndpoint publishEndpoint, ILogger<RecadoCriadoEventConsumer> logger)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RecadoCriadoEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation(
            "📦 Recebido evento RecadoCriado - RecadoId: {RecadoId}, Destinatario: {Destinatario}",
            evento.RecadoId, evento.Destinatario);

        var resultado = await _mediator.Send(
            new ProcessarEntregaCommand(evento.RecadoId, evento.Destinatario, evento.EnderecoEntrega),
            context.CancellationToken);

        if (resultado.Entregue)
        {
            await _publishEndpoint.Publish(new EntregaConcluidaEvent
            {
                RecadoId = evento.RecadoId,
                Destinatario = evento.Destinatario,
                EntregueEm = DateTime.UtcNow
            }, context.CancellationToken);

            await EnviarNotificacaoAsync(
                evento, TipoNotificacao.EntregaConcluida,
                $"Seu recado para {evento.Destinatario} foi entregue!",
                context.CancellationToken);

            return;
        }

        if (!resultado.TentativaFinal)
        {
            _logger.LogWarning(
                "Entrega ainda não concluída (tentativa {Tentativas}), acionando retry do MassTransit - RecadoId: {RecadoId}",
                resultado.Tentativas, evento.RecadoId);

            // Relança para o middleware de retry/circuit breaker reprocessar a mensagem
            throw new InvalidOperationException(resultado.Motivo ?? "Falha ao processar entrega");
        }

        await _publishEndpoint.Publish(new EntregaFalhouEvent
        {
            RecadoId = evento.RecadoId,
            Destinatario = evento.Destinatario,
            FalhouEm = DateTime.UtcNow,
            MotivoFalha = resultado.Motivo ?? "Falha desconhecida",
            TentativasRealizadas = resultado.Tentativas,
            DeveReitentar = false
        }, context.CancellationToken);

        await EnviarNotificacaoAsync(
            evento, TipoNotificacao.EntregaFalhou,
            $"Não foi possível entregar seu recado para {evento.Destinatario} após {resultado.Tentativas} tentativas.",
            context.CancellationToken);
    }

    private async Task EnviarNotificacaoAsync(
        RecadoCriadoEvent evento, TipoNotificacao tipo, string mensagem, CancellationToken cancellationToken)
    {
        await _publishEndpoint.Publish(new EnviarNotificacaoCommand
        {
            RecadoId = evento.RecadoId,
            Destinatario = evento.Destinatario,
            Remetente = evento.Remetente,
            Tipo = tipo,
            Mensagem = mensagem
        }, cancellationToken);
    }
}
