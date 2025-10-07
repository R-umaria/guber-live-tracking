namespace Guber.CoordinatesApi.Models;

public record GeocodeResult(double Latitude, double Longitude, string DisplayName);

public record RouteRequest(
    double StartLat, double StartLon,
    double EndLat, double EndLon);

public record RouteResponse(
    double DistanceKm,
    double DurationMinutes,
    string Polyline // polyline6 string suitable for map rendering
);

public record FareRequest(double DistanceKm);
public record FareResponse(double BaseFare, double PerKm, double DistanceKm, double TotalFare);

public record LiveLocationUpdate(string EntityId, double Lat, double Lon, DateTimeOffset Timestamp);

public record LastLocationResponse(string EntityId, double Lat, double Lon, DateTimeOffset Timestamp);

public record EstimateRequest(string PickupAddress, string DestinationAddress);

public record EstimateResponse(
    string PickupAddress,
    string DestinationAddress,
    double PickupLat,
    double PickupLon,
    double DestinationLat,
    double DestinationLon,
    double DistanceKm,
    double DurationMinutes,
    double Fare,
    string Polyline
);
