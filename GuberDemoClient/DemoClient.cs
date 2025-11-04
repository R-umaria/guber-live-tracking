using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;

public class DemoClient
{
    static readonly HttpClient client = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5157"),
        Timeout = TimeSpan.FromSeconds(30)
    };

    static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task Main()
    {
        Console.WriteLine("=== Guber Demo Client ===\n");

        Console.Write("Enter start address: ");
        string startAddress = (Console.ReadLine() ?? "").Trim();

        Console.Write("Enter destination address: ");
        string endAddress = (Console.ReadLine() ?? "").Trim();

        // 1) Geocode (GET /api/geocode?query=...)
        Console.WriteLine("\nFetching coordinates...");
        var startGeo = await client.GetFromJsonAsync<GeoResponse>($"/api/geocode?query={Uri.EscapeDataString(startAddress)}", JsonOpts);
        var endGeo   = await client.GetFromJsonAsync<GeoResponse>($"/api/geocode?query={Uri.EscapeDataString(endAddress)}", JsonOpts);

        if (startGeo is null || endGeo is null)
        {
            Console.WriteLine("Could not get coordinates (null response).");
            return;
        }

        Console.WriteLine($"Start: ({startGeo.Latitude}, {startGeo.Longitude})");
        Console.WriteLine($"End:   ({endGeo.Latitude}, {endGeo.Longitude})");

        // 2) Estimate (POST /api/estimate) — requires pickupAddress & destinationAddress
        Console.WriteLine("\nEstimating trip...");
        var estimateReq = new
        {
            pickupAddress = startAddress,
            destinationAddress = endAddress
            // If your API accepts coords as well, you can include them:
            // startLat = startGeo.Latitude, startLon = startGeo.Longitude,
            // endLat = endGeo.Latitude,   endLon = endGeo.Longitude
        };

        var estHttp = await client.PostAsJsonAsync("/api/estimate", estimateReq);
        if (!estHttp.IsSuccessStatusCode)
        {
            Console.WriteLine($"Estimate failed: {estHttp.StatusCode}");
            Console.WriteLine(await estHttp.Content.ReadAsStringAsync());
            return;
        }

        var estimate = await estHttp.Content.ReadFromJsonAsync<EstimateResponse>(JsonOpts) ?? new EstimateResponse();
        Console.WriteLine($"Estimated Distance: {estimate.DistanceKm:F2} km");
        Console.WriteLine($"Estimated Duration: {estimate.DurationMinutes:F1} min");
        Console.WriteLine($"Estimated Cost:     ${estimate.Fare:F2}");

        Console.Write("\nStart trip simulation? (y/n): ");
        if (!string.Equals(Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Trip cancelled.");
            return;
        }

        // 3) Route (POST /api/route) — returns { distanceKm, durationMinutes, polyline }
        Console.WriteLine("\nGetting route path...");
        var routeReq = new
        {
            startLat = startGeo.Latitude,
            startLon = startGeo.Longitude,
            endLat   = endGeo.Latitude,
            endLon   = endGeo.Longitude
        };

        var routeHttp = await client.PostAsJsonAsync("/api/route", routeReq);
        if (!routeHttp.IsSuccessStatusCode)
        {
            Console.WriteLine($"Route request failed: {routeHttp.StatusCode}");
            Console.WriteLine(await routeHttp.Content.ReadAsStringAsync());
            return;
        }

        var routeWire = await routeHttp.Content.ReadFromJsonAsync<RouteWire>(JsonOpts);
        if (routeWire is null || string.IsNullOrWhiteSpace(routeWire.Polyline))
        {
            Console.WriteLine("Route returned no polyline.");
            return;
        }

        // Decode polyline6 (precision = 1e-6)
        var points = DecodePolyline6(routeWire.Polyline);
        if (points.Count == 0)
        {
            Console.WriteLine("Decoded polyline returned 0 points.");
            return;
        }

        Console.WriteLine($"Route distance: {routeWire.DistanceKm:F2} km, duration: {routeWire.DurationMinutes:F1} min");
        Console.WriteLine($"Route has {points.Count} decoded points.\n");

        // 4) Simulate movement and push to /api/liveLocation/driver
        double totalKm = 0;
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            Console.WriteLine($"Driver location: {p.Lat:0.0000}, {p.Lon:0.0000}");

            var liveUpdate = new { driverId = "demo-driver", latitude = p.Lat, longitude = p.Lon };
            _ = await client.PostAsJsonAsync("/api/liveLocation/driver", liveUpdate);

            if (i > 0) totalKm += Haversine(points[i - 1], points[i]);
            await Task.Delay(1000);
        }

        var last = points[^1];

        // 5) Final fare (POST /api/fare)
        var fareReq = new
        {
            distanceKm = Math.Max(totalKm, 0.01),
            durationMinutes = Math.Max(routeWire.DurationMinutes, 0.01)
        };

        var fareHttp = await client.PostAsJsonAsync("/api/fare", fareReq);
        if (!fareHttp.IsSuccessStatusCode)
        {
            Console.WriteLine($"Fare request failed: {fareHttp.StatusCode}");
            Console.WriteLine(await fareHttp.Content.ReadAsStringAsync());
            return;
        }

        var finalFare = await fareHttp.Content.ReadFromJsonAsync<FareResponse>(JsonOpts);

        Console.WriteLine("\n=== Trip Complete ===");
        Console.WriteLine($"Km Traveled:  {totalKm:F2}");
        Console.WriteLine($"Total Amount: ${finalFare?.TotalFare:F2}");
        Console.WriteLine($"Last Location: {last.Lat:0.0000}, {last.Lon:0.0000}");
    }

    // ---------- Helpers ----------

    // Haversine in km
    static double Haversine(RoutePoint a, RoutePoint b)
    {
        const double R = 6371;
        double dLat = (b.Lat - a.Lat) * Math.PI / 180.0;
        double dLon = (b.Lon - a.Lon) * Math.PI / 180.0;
        double lat1 = a.Lat * Math.PI / 180.0;
        double lat2 = b.Lat * Math.PI / 180.0;
        double h = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
        return 2 * R * Math.Asin(Math.Sqrt(h));
    }

    // Decode OSRM/Google polyline with 1e-6 precision
    static List<RoutePoint> DecodePolyline6(string polyline)
    {
        var points = new List<RoutePoint>();
        int index = 0, lat = 0, lon = 0;

        while (index < polyline.Length)
        {
            int b, shift = 0, result = 0;
            do { b = polyline[index++] - 63; result |= (b & 0x1f) << shift; shift += 5; } while (b >= 0x20);
            int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
            lat += dlat;

            shift = 0; result = 0;
            do { b = polyline[index++] - 63; result |= (b & 0x1f) << shift; shift += 5; } while (b >= 0x20);
            int dlon = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
            lon += dlon;

            // precision 1e-6
            points.Add(new RoutePoint { Lat = lat / 1e6, Lon = lon / 1e6 });
        }
        return points;
    }

    // ---------- Models ----------

    public class GeoResponse
    {
        [JsonPropertyName("latitude")]  public double Latitude  { get; set; }
        [JsonPropertyName("longitude")] public double Longitude { get; set; }
        [JsonPropertyName("displayName")] public string DisplayName { get; set; } = "";
    }

    // Wire model for /api/route
    public class RouteWire
    {
        [JsonPropertyName("distanceKm")]      public double DistanceKm { get; set; }
        [JsonPropertyName("durationMinutes")] public double DurationMinutes { get; set; }
        [JsonPropertyName("polyline")]        public string Polyline { get; set; } = "";
    }

    public class RoutePoint { public double Lat { get; set; } public double Lon { get; set; } }

    // Exact names from your estimate response
    public class EstimateResponse
    {
        [JsonPropertyName("fare")]            public double Fare { get; set; }
        [JsonPropertyName("distanceKm")]      public double DistanceKm { get; set; }
        [JsonPropertyName("durationMinutes")] public double DurationMinutes { get; set; }
    }

    public class FareResponse { public double TotalFare { get; set; } }
}
