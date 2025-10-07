using Guber.CoordinatesApi.Models;

namespace Guber.CoordinatesApi.Services;

public interface IGeocodingService
{
    Task<GeocodeResult?> GeocodeAsync(string query, CancellationToken ct = default);
}

public interface IRoutingService
{
    Task<RouteResponse> GetRouteAsync(RouteRequest req, CancellationToken ct = default);
}

public interface IFareService
{
    FareResponse Calculate(double distanceKm);
}

public interface ILocationStore
{
    void Upsert(string entityId, double lat, double lon, DateTimeOffset ts);
    LastLocationResponse? Get(string entityId);
}
