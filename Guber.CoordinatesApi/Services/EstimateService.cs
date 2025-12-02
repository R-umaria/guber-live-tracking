using Guber.CoordinatesApi.Models;

namespace Guber.CoordinatesApi.Services;

public interface IEstimateService
{
    Task<EstimateResponse> GetEstimateAsync(EstimateRequest req, CancellationToken ct = default);
}

public sealed class EstimateService : IEstimateService
{
    private readonly IGeocodingService _geo;
    private readonly IRoutingService _route;
    private readonly IFareService _fare;
    private readonly ILogger<EstimateService> _logger;

    public EstimateService(
        IGeocodingService geo,
        IRoutingService route,
        IFareService fare,
        ILogger<EstimateService> logger)
    {
        _geo = geo;
        _route = route;
        _fare = fare;
        _logger = logger;
    }

    public async Task<EstimateResponse> GetEstimateAsync(EstimateRequest req, CancellationToken ct = default)
    {
        _logger.LogInformation("Estimating trip from {pickup} to {destination}", req.PickupAddress, req.DestinationAddress);

        // Step 1: Geocode pickup
        var pickup = await _geo.GeocodeAsync(req.PickupAddress, ct);
        if (pickup is null)
            throw new InvalidOperationException($"Could not geocode pickup: {req.PickupAddress}");

        // Step 2: Geocode destination
        var dest = await _geo.GeocodeAsync(req.DestinationAddress, ct);
        if (dest is null)
            throw new InvalidOperationException($"Could not geocode destination: {req.DestinationAddress}");

        // Step 3: Get route
        var routeReq = new RouteRequest(pickup.Latitude, pickup.Longitude, dest.Latitude, dest.Longitude);
        var route = await _route.GetRouteAsync(routeReq, ct);

        // Step 4: Calculate fare
        var fare = _fare.Calculate(route.DistanceKm);

        // Step 5: Return combined response
        return new EstimateResponse(
            req.PickupAddress,
            req.DestinationAddress,
            pickup.Latitude,
            pickup.Longitude,
            dest.Latitude,
            dest.Longitude,
            route.DistanceKm,
            route.DurationMinutes,
            fare.TotalFare,
            route.Polyline,
            route.Directions
        );
    }
}
