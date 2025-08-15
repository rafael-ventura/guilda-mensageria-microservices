using GuildaMensageria.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace NotificationService.Integration.CommandsIn;

/// <summary>
/// Consumer que processa comandos de envio de notificação
/// </summary>
public class EnviarNotificacaoCommandConsumer : IConsumer<EnviarNotificacaoCommand>
{
    private readonly ILogger<EnviarNotificacaoCommandConsumer> _logger;

    public EnviarNotificacaoCommandConsumer(ILogger<EnviarNotificacaoCommandConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EnviarNotificacaoCommand> context)
    {
        var comando = context.Message;
        
        _logger.LogInformation(
            "🔔 Recebido comando EnviarNotificacao - RecadoId: {RecadoId}, Tipo: {Tipo}, Destinatario: {Destinatario}",
            comando.RecadoId, comando.Tipo, comando.Destinatario);

        try
        {
            // TODO: Implementar lógica de envio de notificação (Email, SMS, Push, etc.)
            // Por enquanto, apenas simula o envio
            await Task.Delay(200, context.CancellationToken);
            
            _logger.LogInformation(
                "✅ Notificação enviada com sucesso - RecadoId: {RecadoId}, Tipo: {Tipo}, Mensagem: {Mensagem}",
                comando.RecadoId, comando.Tipo, comando.Mensagem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "❌ Erro ao enviar notificação para RecadoId: {RecadoId}", 
                comando.RecadoId);
            throw; // Re-throw para acionar retry policy
        }
    }
}
