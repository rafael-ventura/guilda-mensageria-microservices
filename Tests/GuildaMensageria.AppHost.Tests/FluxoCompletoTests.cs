using System.Net.Http.Json;
using InboxService.Application.Queries;
using Microsoft.Extensions.Logging;

namespace GuildaMensageria.AppHost.Tests.Tests;

/// <summary>
/// Smoke test de ponta a ponta via o AppHost do Aspire: sobe RabbitMQ, SQL Server e os
/// 5 serviços de verdade (containers), cria um recado pela API do Dispatch e espera o
/// pipeline inteiro rodar (Outbox → RabbitMQ → DeliveryService → RabbitMQ →
/// InboxService) até a timeline do destinatário refletir um status final.
///
/// Requer Docker Desktop rodando. Não foi executado nesta sessão (Docker indisponível
/// na máquina) — só validado que compila. Rodar com `dotnet test` quando o Docker
/// estiver de pé.
/// </summary>
public class FluxoCompletoTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(90);

    [Fact]
    public async Task RecadoCriado_deve_aparecer_na_timeline_com_status_final()
    {
        var cancellationToken = CancellationToken.None;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.GuildaMensageria_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("dispatchservice-api", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("inboxservice-api", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        using var dispatchClient = app.CreateHttpClient("dispatchservice-api");
        using var inboxClient = app.CreateHttpClient("inboxservice-api");

        const string destinatario = "guilda-teste-e2e";

        using var criarResponse = await dispatchClient.PostAsJsonAsync("/api/recados", new
        {
            remetente = "Teste-Aspire",
            destinatario,
            conteudo = "Mensagem do teste de integração",
            enderecoEntrega = "Rua dos Testes, 123"
        }, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);

        // Dá tempo para o Outbox publicar, o Delivery processar (com possível retry) e o
        // Inbox materializar o status final na timeline.
        var prazo = DateTime.UtcNow.Add(DefaultTimeout);
        ItemTimelineDto? item = null;

        while (DateTime.UtcNow < prazo)
        {
            var timeline = await inboxClient.GetFromJsonAsync<List<ItemTimelineDto>>(
                $"/api/inbox/{destinatario}", cancellationToken);

            item = timeline?.FirstOrDefault();
            if (item is not null && item.Status is "Entregue" or "Falhou")
                break;

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        Assert.NotNull(item);
        Assert.True(item!.Status is "Entregue" or "Falhou",
            $"Esperava status final (Entregue/Falhou) na timeline, mas ficou em '{item.Status}' após {DefaultTimeout.TotalSeconds}s");
    }
}
