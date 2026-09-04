using DispatchService.Domain.Repositories;
using GuildaMensageria.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DispatchService.Infrastructure.Outbox;

/// <summary>
/// Lê periodicamente as OutboxMessages pendentes e publica no barramento — fecha o
/// Outbox Pattern (sem isso, nada gravado no Outbox chega a sair pela mensageria).
/// </summary>
public class OutboxPublisherService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar OutboxMessages pendentes");
            }

            await Task.Delay(PollInterval, stoppingToken).ContinueWith(_ => { });
        }
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var pendentes = await unitOfWork.OutboxMessages.GetPendentesAsync(cancellationToken: cancellationToken);

        foreach (var mensagem in pendentes)
        {
            try
            {
                var publicado = mensagem.EventType switch
                {
                    nameof(RecadoCriadoEvent) => await PublishAsync<RecadoCriadoEvent>(mensagem, publishEndpoint, cancellationToken),
                    _ => LogTipoDesconhecido(mensagem.EventType)
                };

                if (publicado)
                {
                    mensagem.MarcarComoProcessado();
                    await unitOfWork.OutboxMessages.UpdateAsync(mensagem, cancellationToken);
                    _logger.LogInformation(
                        "📤 OutboxMessage publicada - Id: {Id}, EventType: {EventType}",
                        mensagem.Id, mensagem.EventType);
                }
            }
            catch (Exception ex)
            {
                var proximaTentativa = TimeSpan.FromSeconds(5 * Math.Pow(2, mensagem.TentativasProcessamento));
                mensagem.RegistrarTentativaFalhou(ex.Message, proximaTentativa);
                await unitOfWork.OutboxMessages.UpdateAsync(mensagem, cancellationToken);

                _logger.LogError(ex,
                    "Falha ao publicar OutboxMessage - Id: {Id}, EventType: {EventType}, Tentativa: {Tentativa}",
                    mensagem.Id, mensagem.EventType, mensagem.TentativasProcessamento);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static async Task<bool> PublishAsync<TEvent>(
        Domain.Entities.OutboxMessage mensagem,
        IPublishEndpoint publishEndpoint,
        CancellationToken cancellationToken)
        where TEvent : class
    {
        var evento = mensagem.DeserializarEventData<TEvent>();
        if (evento is null)
            throw new InvalidOperationException($"Não foi possível desserializar {typeof(TEvent).Name} da OutboxMessage {mensagem.Id}");

        await publishEndpoint.Publish(evento, cancellationToken);
        return true;
    }

    private bool LogTipoDesconhecido(string eventType)
    {
        _logger.LogWarning("EventType desconhecido no Outbox: {EventType} — mensagem não publicada", eventType);
        return false;
    }
}
