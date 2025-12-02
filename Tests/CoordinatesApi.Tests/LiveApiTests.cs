using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace CoordinatesApi.Tests;

public class LiveApiTests
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    public LiveApiTests()
    {
        _http = new HttpClient() { BaseAddress = TestSettings.BaseUri };
        _json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // Get a valid JWT token dynamically and set it for all requests
        var token = GetJwtTokenAsync().GetAwaiter().GetResult();
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> GetJwtTokenAsync(string userId = "testuser", string role = "user")
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/token", new { UserId = userId, Role = role });
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString()!;
    }

    [Fact(DisplayName = "Health: /health returns 200 and status ok")]
    public async Task Health_ShouldReturnOk()
    {
        var resp = await _http.GetAsync("/health");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
        json.GetProperty("status").GetString().Should().Be("ok");
    }

    [Fact(DisplayName = "Geocode: returns latitude & longitude for Conestoga College")]
    public async Task Geocode_ShouldReturnLatLon()
    {
        var resp = await _http.GetAsync("/api/geocode?query=" + Uri.EscapeDataString("Conestoga College Waterloo"));
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
        json.TryGetProperty("latitude", out var lat).Should().BeTrue();
        json.TryGetProperty("longitude", out var lon).Should().BeTrue();
        lat.GetDouble().Should().BeGreaterThan(40).And.BeLessThan(60);
        lon.GetDouble().Should().BeLessThan(-70).And.BeGreaterThan(-90);
    }

    [Fact(DisplayName = "Fare: 2.4 km ≈ $8.33 (±$0.40)")]
    public async Task Fare_ShouldReturnValidAmount()
    {
        var payload = JsonContent.Create(new { DistanceKm = 2.4 });
        var resp = await _http.PostAsync("/api/fare", payload);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
        var fare = json.GetProperty("totalFare").GetDouble();
        fare.Should().BeInRange(7.9, 8.7);
    }

    [Fact(DisplayName = "Route: returns distance & duration between two Waterloo points")]
    public async Task Route_ShouldReturnDistanceAndDuration()
    {
        var req = new
        {
            StartLat = 43.4723,
            StartLon = -80.5449,
            EndLat = 43.4899,
            EndLon = -80.5280
        };
        var resp = await _http.PostAsJsonAsync("/api/route", req);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
        json.TryGetProperty("distanceKm", out var dist).Should().BeTrue();
        json.TryGetProperty("durationMinutes", out var dur).Should().BeTrue();
        dist.GetDouble().Should().BeGreaterThan(1);
        dur.GetDouble().Should().BeGreaterThan(1);
    }

    [Fact(DisplayName = "Estimate: returns distance, duration, fare & polyline for two addresses")]
    public async Task Estimate_ShouldReturnAll()
    {
        var req = new
        {
            pickupAddress = "Conestoga College, Waterloo, ON",
            destinationAddress = "Conestoga Mall, Waterloo, ON"
        };
        var resp = await _http.PostAsJsonAsync("/api/estimate", req);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);

        json.TryGetProperty("distanceKm", out var dist).Should().BeTrue();
        json.TryGetProperty("durationMinutes", out var dur).Should().BeTrue();
        json.TryGetProperty("directions", out var dirs).Should().BeTrue();
        json.TryGetProperty("fare", out var fare).Should().BeTrue();
        json.TryGetProperty("polyline", out var poly).Should().BeTrue();

        dist.GetDouble().Should().BeGreaterThan(1);
        dur.GetDouble().Should().BeGreaterThan(1);
        dirs.ValueKind.Should().Be(JsonValueKind.Array);
        dirs.GetArrayLength().Should().BeGreaterThan(1);
        var first = dirs.EnumerateArray().First();
        first.TryGetProperty("lat", out var lat0).Should().BeTrue();
        first.TryGetProperty("lon", out var lon0).Should().BeTrue();
        fare.GetDouble().Should().BeGreaterThan(5);
        poly.GetString().Should().NotBeNullOrWhiteSpace();
    }
}
