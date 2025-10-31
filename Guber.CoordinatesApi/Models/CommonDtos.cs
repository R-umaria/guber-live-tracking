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

public record FareRequest(double DistanceKm, string type, bool pet);
public record FareResponse(double BaseFare, double PerKm, double typePrice, double petFee, double DistanceKm, double TotalFare);

public record LiveLocationUpdate(string EntityId, double Lat, double Lon, DateTimeOffset Timestamp);

public record LastLocationResponse(string EntityId, double Lat, double Lon, DateTimeOffset Timestamp);

public record EstimateRequest(string PickupAddress, string DestinationAddress, string type, bool pet);

public record EstimateResponse(
    string PickupAddress,
    string DestinationAddress,
    double PickupLat,
    double PickupLon,
    double DestinationLat,
    double DestinationLon,
    string type,
    bool pet,
    double DistanceKm,
    double DurationMinutes,
    double Fare,
    string Polyline
);
