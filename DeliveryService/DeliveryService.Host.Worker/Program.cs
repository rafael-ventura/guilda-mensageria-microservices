using DeliveryService.Integration.Topology;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// Aspire: service discovery, resilience, health checks e OpenTelemetry
builder.AddServiceDefaults();

// MediatR - Commands/Queries/Notifications
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssemblyContaining<DeliveryService.Application.AssemblyMarker>();
});

// EF Core + SQL Server
builder.Services.AddDbContext<DeliveryService.Infrastructure.Data.DeliveryDbContext>(options =>
{
    // Aspire injeta "GuildaDelivery" (nome do recurso do AppHost); fora do Aspire, usa appsettings
    var connectionString = builder.Configuration.GetConnectionString("GuildaDelivery")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Repository Pattern + Unit of Work
builder.Services.AddScoped<DeliveryService.Domain.Repositories.IUnitOfWork, DeliveryService.Infrastructure.Repositories.UnitOfWork>();

// MassTransit - Mensageria com RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registrar consumers
    x.AddConsumersFromNamespaceContaining<DeliveryService.Integration.EventsIn.RecadoCriadoEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
        var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq");

        if (!string.IsNullOrEmpty(rabbitConnectionString))
        {
            // Injetado pelo Aspire AppHost (recurso "rabbitmq")
            cfg.Host(new Uri(rabbitConnectionString));
        }
        else
        {
            cfg.Host(rabbitConfig["Host"], rabbitConfig["VirtualHost"], h =>
            {
                h.Username(rabbitConfig["Username"] ?? "guest");
                h.Password(rabbitConfig["Password"] ?? "guest");
            });
        }

        // Retry + Circuit Breaker na tentativa de entrega
        cfg.UseMessageRetry(r => r.Intervals(
            MessagingTopology.RetryPolicy.RetryIntervals.Select(s => TimeSpan.FromSeconds(s)).ToArray()));

        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 50;
            cb.ActiveThreshold = 5;
            cb.ResetInterval = TimeSpan.FromMinutes(1);
        });

        cfg.ConfigureEndpoints(context);
    });
});

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
