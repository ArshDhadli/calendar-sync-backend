using Microsoft.AspNetCore.Mvc;

namespace SyncBackend.Api.Controllers;

/// <summary>
/// Liveness endpoint. A monitoring system (or you, or later AWS) can hit this
/// to confirm the API process is up and serving requests. It deliberately does
/// no real work (no DB call yet) so "is the web server alive?" and "is the
/// database reachable?" stay separate concerns.
/// </summary>
[ApiController]
[Route("[controller]")] // -> GET /health
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "ok",
        service = "SyncBackend.Api",
        timeUtc = DateTime.UtcNow,
    });
}
