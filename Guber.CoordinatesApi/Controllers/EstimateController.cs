using Guber.CoordinatesApi.Models;
using Guber.CoordinatesApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guber.CoordinatesApi.Controllers;

[Authorize]
[ApiController]
[Route("api/estimate")]
public sealed class EstimateController : ControllerBase
{
    private readonly IEstimateService _estimate;
    private readonly ILogger<EstimateController> _logger;

    public EstimateController(IEstimateService estimate, ILogger<EstimateController> logger)
    {
        _estimate = estimate;
        _logger = logger;
    }

    /// <summary>
    /// Estimate route, distance, time, and fare between two addresses.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Estimate([FromBody] EstimateRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.PickupAddress) || string.IsNullOrWhiteSpace(req.DestinationAddress))
            return BadRequest(new { error = "Both pickupAddress and destinationAddress are required." });

        try
        {
            var result = await _estimate.GetEstimateAsync(req, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating route.");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
