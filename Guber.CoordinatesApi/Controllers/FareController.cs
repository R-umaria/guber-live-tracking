using Guber.CoordinatesApi.Models;
using Guber.CoordinatesApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Guber.CoordinatesApi.Controllers;

[ApiController]
[Route("api/fare")]
public sealed class FareController : ControllerBase
{
    private readonly IFareService _fare;
    public FareController(IFareService fare) => _fare = fare;

    /// <summary>Calculate fare from distance in km. Additonally Add car type and pet friendly</summary>
    /// 
    [HttpPost]
    public ActionResult<FareResponse> Calc([FromBody] FareRequest req)
    {
        if (req.DistanceKm < 0) return BadRequest(new { error = "DistanceKm must be >= 0" });
        if (req.type != "X" && req.type != "XL") return BadRequest(new { error = "type must be X or XL" });

        return Ok(_fare.Calculate(req.DistanceKm, req.type, req.pet ));
    }
}
