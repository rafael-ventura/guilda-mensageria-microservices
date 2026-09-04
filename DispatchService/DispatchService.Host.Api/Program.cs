using Serilog;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Aspire: service discovery, resilience, health checks e OpenTelemetry
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR - Commands/Queries/Notifications
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssemblyContaining<DispatchService.Application.AssemblyMarker>();
});

// MassTransit - Mensageria com RabbitMQ
builder.Services.AddMassTransit(x =>
{
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

        cfg.ConfigureEndpoints(context);
    });
});

// EF Core + SQL Server
builder.Services.AddDbContext<DispatchService.Infrastructure.Data.DispatchDbContext>(options =>
{
    // Aspire injeta "GuildaDispatch" (nome do recurso do AppHost); fora do Aspire, usa appsettings
    var connectionString = builder.Configuration.GetConnectionString("GuildaDispatch")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Repository Pattern + Unit of Work
builder.Services.AddScoped<DispatchService.Domain.Repositories.IUnitOfWork, DispatchService.Infrastructure.Repositories.UnitOfWork>();

// Outbox Pattern - publica no barramento as mensagens gravadas pela transação de escrita
builder.Services.AddHostedService<DispatchService.Infrastructure.Outbox.OutboxPublisherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();

try
{
    Log.Information("Iniciando DispatchService.Host.Api");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}
