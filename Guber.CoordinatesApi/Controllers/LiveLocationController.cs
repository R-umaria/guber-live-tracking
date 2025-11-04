using Guber.CoordinatesApi.Models;
using Guber.CoordinatesApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Guber.CoordinatesApi.Controllers;

[Authorize]
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

        // Authorization check: Ensure the entityId from the request matches the authenticated user.
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (authenticatedUserId == null || !authenticatedUserId.Equals($"driver:{update.EntityId}", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid(); // Return 403 Forbidden
        }

        _store.Upsert($"driver:{update.EntityId}", update.Lat, update.Lon, update.Timestamp == default ? DateTimeOffset.UtcNow : update.Timestamp);
        return Ok(new { status = "updated" });
    }

    /// <summary>Update user's location.</summary>
    [HttpPost("liveLocation/user")]
    public IActionResult UpdateUser([FromBody] LiveLocationUpdate update)
    {
        if (string.IsNullOrWhiteSpace(update.EntityId))
            return BadRequest(new { error = "EntityId required" });

        // Authorization check: Ensure the entityId from the request matches the authenticated user.
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (authenticatedUserId == null || !authenticatedUserId.Equals($"user:{update.EntityId}", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid(); // Return 403 Forbidden
        }

        _store.Upsert($"user:{update.EntityId}", update.Lat, update.Lon, update.Timestamp == default ? DateTimeOffset.UtcNow : update.Timestamp);
        return Ok(new { status = "updated" });
    }

    /// <summary>Get last known location for a driver or user.</summary>
    [HttpGet("lastLocation")]
    public ActionResult<LastLocationResponse> Last([FromQuery] string entityType, [FromQuery] string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
            return BadRequest(new { error = "entityType and entityId required" });

        // Authorization check: Allow users to retrieve their own data only.
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requestedKey = $"{entityType}:{entityId}".ToLowerInvariant();

        if (authenticatedUserId == null || !authenticatedUserId.Equals(requestedKey, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid(); // Return 403 Forbidden
        }

        var res = _store.Get(requestedKey);
        return res is null ? NotFound(new { error = "Not found" }) : Ok(res);
    }
}