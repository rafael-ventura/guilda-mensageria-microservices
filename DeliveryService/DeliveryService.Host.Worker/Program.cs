using Serilog;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

// Configurar Serilog
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
    cfg.RegisterServicesFromAssemblyContaining<DeliveryService.Application.AssemblyMarker>();
});

// MassTransit - Mensageria com RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registrar consumers
    x.AddConsumersFromNamespaceContaining<DeliveryService.Integration.EventsIn.RecadoCriadoEventConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
        
        cfg.Host(rabbitConfig["Host"], rabbitConfig["VirtualHost"], h =>
        {
            h.Username(rabbitConfig["Username"]);
            h.Password(rabbitConfig["Password"]);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// TODO: Adicionar EF Core

var host = builder.Build();

try
{
    Log.Information("Iniciando DeliveryService.Host.Worker");
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
