using Microsoft.AspNetCore.Mvc;

namespace Guber.CoordinatesApi.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok", at = DateTimeOffset.UtcNow });
}
