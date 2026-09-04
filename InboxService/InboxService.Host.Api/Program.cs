using InboxService.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR - Queries (lado de leitura do CQRS)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<InboxService.Application.AssemblyMarker>();
});

// EF Core + SQL Server
builder.Services.AddDbContext<InboxService.Infrastructure.Data.InboxDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Repository Pattern + Unit of Work
builder.Services.AddScoped<InboxService.Domain.Repositories.IUnitOfWork, InboxService.Infrastructure.Repositories.UnitOfWork>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/inbox/{destinatario}", async (string destinatario, IMediator mediator, CancellationToken cancellationToken) =>
{
    var timeline = await mediator.Send(new ObterTimelineQuery(destinatario), cancellationToken);
    return Results.Ok(timeline);
})
.WithName("ObterTimeline");

app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Service = "InboxService", Timestamp = DateTime.UtcNow }));

try
{
    Log.Information("Iniciando InboxService.Host.Api");
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
