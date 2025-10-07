using System.Text.Json;
using Guber.CoordinatesApi.Models;

namespace Guber.CoordinatesApi.Services;

public sealed class OsrmRoutingService : IRoutingService
{
    private readonly HttpClient _http;
    public OsrmRoutingService(HttpClient http) => _http = http;

    private sealed class OsrmGeometry
    {
        public string? geometry { get; set; } // polyline6
        public double distance { get; set; }  // meters
        public double duration { get; set; }  // seconds
    }
    private sealed class OsrmRouteResponse
    {
        public string? code { get; set; }
        public List<OsrmGeometry>? routes { get; set; }
    }

    public async Task<RouteResponse> GetRouteAsync(RouteRequest req, CancellationToken ct = default)
    {
        // OSRM wants lon,lat order
        var path = $"/route/v1/driving/{req.StartLon},{req.StartLat};{req.EndLon},{req.EndLat}?overview=full&geometries=polyline6";
        var json = await _http.GetStringAsync(path, ct);

        var parsed = JsonSerializer.Deserialize<OsrmRouteResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var route = parsed?.routes?.FirstOrDefault();
        if (parsed?.code != "Ok" || route == null || string.IsNullOrWhiteSpace(route.geometry))
            throw new InvalidOperationException("Route not found");

        var km = Math.Round(route.distance / 1000.0, 3);
        var minutes = Math.Round(route.duration / 60.0, 2);
        return new RouteResponse(km, minutes, route.geometry!);
    }
}
