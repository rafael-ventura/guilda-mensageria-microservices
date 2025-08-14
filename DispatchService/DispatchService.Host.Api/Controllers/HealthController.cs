using Microsoft.AspNetCore.Mvc;

namespace DispatchService.Host.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check solicitado");
        return Ok(new { 
            Status = "Healthy", 
            Service = "DispatchService",
            Timestamp = DateTime.UtcNow 
        });
    }
}
