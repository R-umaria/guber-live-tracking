using System;

namespace CoordinatesApi.Tests;

internal static class TestSettings
{
    public static readonly Uri BaseUri = new Uri(
        Environment.GetEnvironmentVariable("GUBER_API_BASEURL")
        ?? "http://localhost:5157"
    );
}
