using Guber.CoordinatesApi.Models;
using Guber.CoordinatesApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guber.CoordinatesApi.Controllers;

[Authorize]
[ApiController]
[Route("api/route")]
public sealed class RoutingController : ControllerBase
{
    private readonly IRoutingService _routing;
    public RoutingController(IRoutingService routing) => _routing = routing;

    /// <summary>Get route, distance and duration between two coordinates.</summary>
    [HttpPost]
    public async Task<ActionResult<RouteResponse>> GetRoute([FromBody] RouteRequest req, CancellationToken ct)
    {
        if (!IsValidCoord(req.StartLat, req.StartLon) || !IsValidCoord(req.EndLat, req.EndLon))
            return BadRequest(new { error = "Invalid coordinates" });

        var route = await _routing.GetRouteAsync(req, ct);
        return Ok(route);
    }

    private static bool IsValidCoord(double lat, double lon)
        => lat is >= -90 and <= 90 && lon is >= -180 and <= 180;
}
