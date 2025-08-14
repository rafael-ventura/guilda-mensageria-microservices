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