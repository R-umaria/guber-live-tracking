using Guber.CoordinatesApi.Models;
using Guber.CoordinatesApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Guber.CoordinatesApi.Controllers;

[ApiController]
[Route("api")]
public sealed class LiveLocationController : ControllerBase
{
    private readonly ILocationStore _store;
    public LiveLocationController(ILocationStore store) => _store = store;

    /// <summary>Update driver's live location.</summary>
    [HttpPost("liveLocation/driver")]
    public IActionResult UpdateDriver([FromBody] LiveLocationUpdate update)
    {
        if (string.IsNullOrWhiteSpace(update.EntityId))
            return BadRequest(new { error = "EntityId required" });

        _store.Upsert($"driver:{update.EntityId}", update.Lat, update.Lon, update.Timestamp == default ? DateTimeOffset.UtcNow : update.Timestamp);
        return Ok(new { status = "updated" });
    }

    /// <summary>Update user's location.</summary>
    [HttpPost("liveLocation/user")]
    public IActionResult UpdateUser([FromBody] LiveLocationUpdate update)
    {
        if (string.IsNullOrWhiteSpace(update.EntityId))
            return BadRequest(new { error = "EntityId required" });

        _store.Upsert($"user:{update.EntityId}", update.Lat, update.Lon, update.Timestamp == default ? DateTimeOffset.UtcNow : update.Timestamp);
        return Ok(new { status = "updated" });
    }

    /// <summary>Get last known location for a driver or user.</summary>
    [HttpGet("lastLocation")]
    public ActionResult<LastLocationResponse> Last([FromQuery] string entityType, [FromQuery] string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
            return BadRequest(new { error = "entityType and entityId required" });

        var key = $"{entityType}:{entityId}".ToLowerInvariant();
        var res = _store.Get(key);
        return res is null ? NotFound(new { error = "Not found" }) : Ok(res);
    }
}
