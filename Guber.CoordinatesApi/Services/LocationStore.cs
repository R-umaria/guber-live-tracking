using System.Collections.Concurrent;
using Guber.CoordinatesApi.Models;

namespace Guber.CoordinatesApi.Services;

public sealed class InMemoryLocationStore : ILocationStore
{
    private readonly ConcurrentDictionary<string, LastLocationResponse> _store = new();

    public void Upsert(string entityId, double lat, double lon, DateTimeOffset ts)
        => _store[entityId] = new LastLocationResponse(entityId, lat, lon, ts);

    public LastLocationResponse? Get(string entityId)
        => _store.TryGetValue(entityId, out var v) ? v : null;
}
