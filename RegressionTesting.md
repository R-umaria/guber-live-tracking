# Regression Testing – Sprint 2 (Coordinates & Fare API)

**Date:** ${DATE}
**Environment:** ${Local|Portainer}
**Base URL:** ${http://localhost:5157 | http://10.89.0.2:5157}

## Test Matrix
| ID | Endpoint | Case | Expected | Result |
|---:|---|---|---|---|
| T1 | /health | Status | 200 + {status:"ok"} | ✅ Pass |
| T2 | /api/geocode | Geocode Conestoga | lat/lon present | ✅ Pass |
| T3 | /api/fare | 2.4km fare | ≈ 8.33 | ✅ Pass |
| T4 | /api/route | Waterloo → Waterloo | distance & duration > 1 | ✅ Pass |
| T5 | /api/estimate | College → Mall | distance, duration, fare, polyline | ✅ Pass |

## Console Output
```
>>> dotnet test -v n

Restore complete (0.3s)
  CoordinatesApi.Tests succeeded (0.4s) → bin\Debug\net8.0\CoordinatesApi.Tests.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.8.2+699d445a1a (64-bit .NET 8.0.20)
[xUnit.net 00:00:00.09]   Discovering: CoordinatesApi.Tests
[xUnit.net 00:00:00.12]   Discovered:  CoordinatesApi.Tests
[xUnit.net 00:00:00.12]   Starting:    CoordinatesApi.Tests
[xUnit.net 00:00:05.69]   Finished:    CoordinatesApi.Tests
  CoordinatesApi.Tests test succeeded (6.5s)

Test summary: total: 5, failed: 0, succeeded: 5, skipped: 0, duration: 6.5s
Build succeeded in 7.8s
```

## Notes
- Tests run with xUnit via `dotnet test`.
- Geocoding and routing rely on external OSM/OSRM services; transient network issues may cause intermittent failures.