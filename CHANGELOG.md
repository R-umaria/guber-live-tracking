# Guber.CoordinatesApi – Changelog

All notable changes to this project are documented here, following [Semantic Versioning](https://semver.org/) guidelines.

---

## [1.3.1] – 2025-11-11
### Maintenance & Documentation
- Removed previously committed build and Doxygen output (`bin/`, `obj/`, `doxygen/`)
- Added hardened `.gitignore` to prevent future binary/docs commits
- Updated **README.md** with full Swagger-based API reference  
- Added this changelog entry for proper release tracking

---

## [1.3.0] – 2025-11-11
### Added
- Introduced **decoded `directions` array** (list of `{ lat, lon }`) to both:
  - `/api/route`
  - `/api/estimate`
- Implemented `PolylineDecoder` utility for decoding OSRM `polyline6` geometry
- Extended `RouteResponse` and `EstimateResponse` DTOs to include `Directions`
- Updated unit and integration tests to validate new property
- Validated API responses via Swagger and curl (Hamilton → Waterloo route)

### Example
```json
{
  "distanceKm": 12.34,
  "durationMinutes": 22.5,
  "polyline": "string",
  "directions": [
    { "lat": 43.4723, "lon": -80.5449 },
    { "lat": 43.4745, "lon": -80.5432 }
  ]
}
```

## Technical
* Rebased feature branch from main (removed Ryan-Additions history)

* Cleaned test variable naming (dirs → decoded)

* Published Git tag `` v1.3.0 `` after successful CI pass

## Upcoming
* Support for ``?includeDirections=false`` query flag to optionally trim large payloads

* Downsampled route option for driver simulation modules

---

## [2.0-sprint2] – 2025-10-13

## Added

* ``/api/estimate`` endpoint combining geocode + route + fare

* Complete Docker & Portainer stack deployment

* Live API xUnit regression test suite (5 passing tests)

* Wiki.js + Swagger integration

### Improved
* Refactored service layer into Services/

* Better structured README and documentation

### Known Issues

* External OSRM dependency (requires internet)
---
## Maintainers
#### Rishi Umaria
#### Brian Nguyen

Last updated: November 11 2025
