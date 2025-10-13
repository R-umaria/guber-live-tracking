# 🧭 Guber – Coordinates & Fare Calculation Module

This module is part of the **Guber (Uber Clone)** enterprise system.  
It handles:
- Address → Coordinates conversion (Geocoding)
- Route calculation and distance estimation
- Fare computation (`Base Fare + Per-Km Rate`)
- Live driver/user location updates
- Simple REST APIs for integration with other teams (UI, Driver Management, Payment)

Built with **C# (.NET 8)** using **OpenStreetMap (Nominatim)** and **OSRM** routing.

---

## Getting Started

### 1 Prerequisites
- **.NET 8 SDK** → [download here](https://dotnet.microsoft.com/download)
- Any editor (Visual Studio 2022 or VS Code with C# Dev Kit)
- Internet connection (for OSM/OSRM APIs)

### 2 Clone the repository
```bash
git clone https://github.com/R-umaria/guber-live-tracking/
cd guber-live-tracking/Guber.CoordinatesApi   
```

### 3 Run the api
```
bash
dotnet restore
dotnet run
```

Once started, you’ll see:

"Now listening on: http://localhost:5157"

Then open:
```http://localhost:5157/swagger```

## API overview

| Endpoint                   | Method | Description                    | Example                                                                        |
| -------------------------- | ------ | ------------------------------ | ------------------------------------------------------------------------------ |
| `/health`                  | GET    | Check if service is running    | ✓                                                                              |
| `/api/geocode`             | GET    | Convert address → lat/lon      | `?query=Conestoga+College`                                                     |
| `/api/route`               | POST   | Get route, distance, duration  | `{ "StartLat": 43.48, "StartLon": -80.52, "EndLat": 43.50, "EndLon": -80.54 }` |
| `/api/fare`                | POST   | Compute fare                   | `{ "DistanceKm": 2.4 }`                                                        |
| `/api/liveLocation/driver` | POST   | Update driver’s live location  | `{ "EntityId": "D001", "Lat": 43.49, "Lon": -80.53 }`                          |
| `/api/liveLocation/user`   | POST   | Update user’s current location | `{ "EntityId": "U001", "Lat": 43.48, "Lon": -80.52 }`                          |
| `/api/lastLocation`        | GET    | Get last known location        | `?entityType=driver&entityId=D001`                                             |
| Endpoint        | Method | Description                                                                                    | Example                                                                                                        |
| `/api/estimate` | POST   | **Takes pickup & destination addresses, returns geocoded route, distance, fare, and polyline** | `{ "pickupAddress": "Conestoga College, Waterloo, ON", "destinationAddress": "Conestoga Mall, Waterloo, ON" }` |

### Fare Formula
Fare = Base Fare ($4.25) + (Distance × $1.70/km)

Example:
If distance = 2.4 km → $4.25 + (2.4 × 1.7) = $8.33\

### Notes

Data is stored in-memory (resets when the app restarts).

Make sure to post at least one live location before fetching /api/lastLocation.

All responses are in JSON and visible in Swagger UI.

Works with other Guber modules via REST.

## Contributors

### Coordinates & Fare Module Team

Rishi Umaria 
