using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Guber.CoordinatesApi; // Program class reference


namespace CoordinatesApi.InMemoryTests;


public class InMemoryApiTests : IClassFixture<WebApplicationFactory<Program>>
{
private readonly HttpClient _client;


public InMemoryApiTests(WebApplicationFactory<Program> factory)
{
_client = factory.CreateClient();
}


[Fact(DisplayName = "Health: in-memory /health works")]
public async Task Health_ShouldReturnOk()
{
var resp = await _client.GetAsync("/health");
resp.EnsureSuccessStatusCode();
var data = await resp.Content.ReadFromJsonAsync<Dictionary<string,string>>();
data!["status"].Should().Be("ok");
}


[Fact(DisplayName = "Fare endpoint returns valid fare value")]
public async Task Fare_ShouldReturnExpectedValue()
{
var payload = JsonContent.Create(new { DistanceKm = 2.4 });
var resp = await _client.PostAsync("/api/fare", payload);
resp.EnsureSuccessStatusCode();
var json = await resp.Content.ReadFromJsonAsync<Dictionary<string,double>>();
json!.Values.First().Should().BeInRange(7.9, 8.7);
}


[Fact(DisplayName = "Estimate endpoint returns fare & distance")]
public async Task Estimate_ShouldReturnComposite()
{
var req = new { pickupAddress = "Conestoga College, Waterloo, ON", destinationAddress = "Conestoga Mall, Waterloo, ON" };
var resp = await _client.PostAsJsonAsync("/api/estimate", req);
resp.EnsureSuccessStatusCode();
var json = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
json!.Should().ContainKey("fare").Or.ContainKey("Fare");
}
}