using MassTransit;
using NotificationService.Domain.Notificacoes;
using NotificationService.Infrastructure.Notificacoes;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// MediatR - Commands/Queries/Notifications
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssemblyContaining<NotificationService.Application.AssemblyMarker>();
});

// Strategy Pattern - canal de notificação (hoje simulado via console/log)
builder.Services.AddScoped<ICanalNotificacao, ConsoleCanalNotificacao>();

// MassTransit - Mensageria com RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registrar consumers
    x.AddConsumersFromNamespaceContaining<NotificationService.Integration.CommandsIn.EnviarNotificacaoCommandConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");

        cfg.Host(rabbitConfig["Host"], rabbitConfig["VirtualHost"], h =>
        {
            h.Username(rabbitConfig["Username"] ?? "guest");
            h.Password(rabbitConfig["Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();

try
{
    Log.Information("Iniciando NotificationService.Host.Worker");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}
