using GuildaMensageria.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DeliveryService.Integration.EventsIn;

/// <summary>
/// Consumer que processa eventos de recado criado para iniciar o processo de entrega
/// </summary>
public class RecadoCriadoEventConsumer : IConsumer<RecadoCriadoEvent>
{
    private readonly ILogger<RecadoCriadoEventConsumer> _logger;

    public RecadoCriadoEventConsumer(ILogger<RecadoCriadoEventConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RecadoCriadoEvent> context)
    {
        var evento = context.Message;
        
        _logger.LogInformation(
            "📦 Recebido evento RecadoCriado - RecadoId: {RecadoId}, Remetente: {Remetente}, Destinatario: {Destinatario}",
            evento.RecadoId, evento.Remetente, evento.Destinatario);

        try
        {
            // TODO: Implementar lógica de processamento de entrega
            // Por enquanto, apenas simula o processamento
            await Task.Delay(1000, context.CancellationToken);
            
            _logger.LogInformation(
                "✅ Processamento de entrega iniciado para RecadoId: {RecadoId}",
                evento.RecadoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "❌ Erro ao processar entrega para RecadoId: {RecadoId}", 
                evento.RecadoId);
            throw; // Re-throw para acionar retry policy
        }
    }
}
