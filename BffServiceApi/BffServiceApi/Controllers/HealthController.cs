using BffApiClassLibrary.Services.Health;
using BffApiClassLibrary.Views;
using Microsoft.AspNetCore.Mvc;

namespace BffServiceApi.Controllers;

[ApiController]
[Route("bff/health")]
public class HealthController(IHealthService healthService) : ControllerBase
{
    private readonly IHealthService healthService = healthService;

    /// <summary>Estado do BFF e de cada microsserviço. 503 so quando nenhum serviço responde.</summary>
    [HttpGet]
    public async Task<ActionResult<HealthView>> Check(CancellationToken ct)
    {
        HealthView health = await healthService.CheckAsync(ct);
        return health.Status == "unhealthy" ? StatusCode(StatusCodes.Status503ServiceUnavailable, health) : Ok(health);
    }
}
