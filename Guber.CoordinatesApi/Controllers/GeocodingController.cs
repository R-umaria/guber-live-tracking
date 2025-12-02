using Guber.CoordinatesApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Guber.CoordinatesApi.Controllers;

[Authorize]
[ApiController]
[Route("api/geocode")]
public sealed class GeocodingController : ControllerBase
{
    private readonly IGeocodingService _geo;
    public GeocodingController(IGeocodingService geo) => _geo = geo;

    /// <summary>Convert an address/place into coordinates.</summary>
    [HttpGet]
    public async Task<IActionResult> Geocode([FromQuery] string query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "query is required" });

        var res = await _geo.GeocodeAsync(query, ct);
        return res is null ? NotFound(new { error = "No result" }) : Ok(res);
    }
}
