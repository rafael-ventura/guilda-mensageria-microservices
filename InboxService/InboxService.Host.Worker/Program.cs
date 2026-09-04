using MassTransit;
using Microsoft.EntityFrameworkCore;
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
    cfg.RegisterServicesFromAssemblyContaining<InboxService.Application.AssemblyMarker>();
});

// EF Core + SQL Server
builder.Services.AddDbContext<InboxService.Infrastructure.Data.InboxDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Repository Pattern + Unit of Work
builder.Services.AddScoped<InboxService.Domain.Repositories.IUnitOfWork, InboxService.Infrastructure.Repositories.UnitOfWork>();

// MassTransit - Mensageria com RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registrar consumers (RecadoCriadoEvent, EntregaConcluidaEvent, EntregaFalhouEvent)
    x.AddConsumersFromNamespaceContaining<InboxService.Integration.EventsIn.RecadoCriadoEventConsumer>();

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
    Log.Information("Iniciando InboxService.Host.Worker");
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
