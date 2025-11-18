using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace CoordinatesApi.Tests;

public class ApiNegativeAndValidationTests
{
    private readonly HttpClient _http = new() { BaseAddress = TestSettings.BaseUri };
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    // 1) Geocode: query is required
    [Fact(DisplayName = "Geocode: empty query returns 400 Bad Request")]
    public async Task Geocode_EmptyQuery_ShouldReturnBadRequest()
    {
        var resp = await _http.GetAsync("/api/geocode?query=");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Just check that there is some response content
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }


    // 2) Geocode: unknown address returns 404
    [Fact(DisplayName = "Geocode: unknown address returns 404 Not Found")]
    public async Task Geocode_UnknownAddress_ShouldReturnNotFound()
    {
        var resp = await _http.GetAsync("/api/geocode?query=asdkjhaskdjhaskdjh123123");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
        json.TryGetProperty("error", out var err).Should().BeTrue();
        err.GetString().Should().NotBeNullOrWhiteSpace();
    }

    // 3) Fare: negative distance not allowed (regression safeguard)
    [Fact(DisplayName = "Fare: negative distance returns 400 and error message")]
    public async Task Fare_NegativeDistance_ShouldReturnBadRequest()
    {
        var request = new { distanceKm = -5.0 };
        var resp = await _http.PostAsJsonAsync("/api/fare", request);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
        json.TryGetProperty("error", out var err).Should().BeTrue();
        err.GetString().Should().Be("DistanceKm must be >= 0");
    }

    // 4) Fare: zero km edge case (regression)
    [Fact(DisplayName = "Fare: zero distance returns base fare only")]
    public async Task Fare_ZeroDistance_ShouldReturnBaseFareOnly()
    {
        var request = new { distanceKm = 0.0 };
        var resp = await _http.PostAsJsonAsync("/api/fare", request);

        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);

        json.TryGetProperty("baseFare", out var baseFare).Should().BeTrue();
        json.TryGetProperty("perKm", out var perKm).Should().BeTrue();
        json.TryGetProperty("distanceKm", out var dist).Should().BeTrue();
        json.TryGetProperty("totalFare", out var total).Should().BeTrue();

        var baseVal = baseFare.GetDouble();
        var perKmVal = perKm.GetDouble();
        var totalVal = total.GetDouble();
        var distVal = dist.GetDouble();

        distVal.Should().Be(0.0);
        totalVal.Should().BeApproximately(baseVal, 0.01);
        perKmVal.Should().BeGreaterThan(0.0);
    }

    // 5) Route: invalid coordinates return 400
    [Fact(DisplayName = "Route: invalid coordinates return 400 Bad Request")]
    public async Task Route_InvalidCoordinates_ShouldReturnBadRequest()
    {
        var request = new
        {
            startLat = 999.0,
            startLon = 0.0,
            endLat = 43.45,
            endLon = -80.49
        };

        var resp = await _http.PostAsJsonAsync("/api/route", request);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
        json.TryGetProperty("error", out var err).Should().BeTrue();
        err.GetString().Should().Be("Invalid coordinates");
    }

    // 6) Estimate: missing addresses returns 400
    [Fact(DisplayName = "Estimate: missing pickup or destination returns 400")]
    public async Task Estimate_MissingAddresses_ShouldReturnBadRequest()
    {
        var request = new
        {
            pickupAddress = "",
            destinationAddress = ""
        };

        var resp = await _http.PostAsJsonAsync("/api/estimate", request);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
        json.TryGetProperty("error", out var err).Should().BeTrue();
        err.GetString().Should().Be("Both pickupAddress and destinationAddress are required.");
    }

    // 7) Smoke/Performance: route completes within an upper bound
    [Fact(DisplayName = "Route: request completes within 2 seconds")]
    public async Task Route_ShouldCompleteWithinTwoSeconds()
    {
        var request = new
        {
            startLat = 43.4723,
            startLon = -80.5449,
            endLat = 43.4516,
            endLon = -80.4925
        };

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var watch = System.Diagnostics.Stopwatch.StartNew();

        var resp = await _http.PostAsJsonAsync("/api/route", request, cts.Token);
        watch.Stop();

        resp.EnsureSuccessStatusCode();
        watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2.0));
    }
}
