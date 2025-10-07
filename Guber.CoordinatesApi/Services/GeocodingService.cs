using System.Net.Http.Json;
using System.Text.Json;
using Guber.CoordinatesApi.Models;

namespace Guber.CoordinatesApi.Services;

public sealed class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _http;

    public NominatimGeocodingService(HttpClient http) => _http = http;

    private sealed class NominatimItem
    {
        public string? display_name { get; set; }
        public string? lat { get; set; }
        public string? lon { get; set; }
    }

    public async Task<GeocodeResult?> GeocodeAsync(string query, CancellationToken ct = default)
    {
        // Nominatim: /search?q=...&format=json&limit=1
        var url = $"/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var res = await _http.GetFromJsonAsync<List<NominatimItem>>(url, opts, ct);

        var first = res?.FirstOrDefault();
        if (first == null || !double.TryParse(first.lat, out var lat) || !double.TryParse(first.lon, out var lon))
            return null;

        return new GeocodeResult(lat, lon, first.display_name ?? query);
    }
}
