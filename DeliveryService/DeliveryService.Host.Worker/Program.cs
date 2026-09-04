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

// MediatR - Commands/Queries/Notifications
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssemblyContaining<DeliveryService.Application.AssemblyMarker>();
});

// EF Core + SQL Server
builder.Services.AddDbContext<DeliveryService.Infrastructure.Data.DeliveryDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
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

        cfg.Host(rabbitConfig["Host"], rabbitConfig["VirtualHost"], h =>
        {
            h.Username(rabbitConfig["Username"] ?? "guest");
            h.Password(rabbitConfig["Password"] ?? "guest");
        });

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
