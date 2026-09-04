using Microsoft.Extensions.Logging;
using NotificationService.Domain.Notificacoes;

namespace NotificationService.Infrastructure.Notificacoes;

/// <summary>
/// Estratégia concreta de envio: simula o envio registrando em log (nenhum provider
/// real ainda). Trocar por Email/SMS/Push no futuro é implementar ICanalNotificacao
/// de novo e registrar no DI — o Application não muda.
/// </summary>
public class ConsoleCanalNotificacao : ICanalNotificacao
{
    private readonly ILogger<ConsoleCanalNotificacao> _logger;

    public string Nome => "Console";

    public ConsoleCanalNotificacao(ILogger<ConsoleCanalNotificacao> logger)
    {
        _logger = logger;
    }

    public async Task EnviarAsync(NotificacaoMensagem mensagem, CancellationToken cancellationToken = default)
    {
        // Simula latência de um provider externo
        await Task.Delay(200, cancellationToken);

        _logger.LogInformation(
            "🔔 [{Canal}] Notificação enviada - RecadoId: {RecadoId}, Destinatario: {Destinatario}, Tipo: {Tipo}, Mensagem: {Mensagem}",
            Nome, mensagem.RecadoId, mensagem.Destinatario, mensagem.Tipo, mensagem.Mensagem);
    }
}
