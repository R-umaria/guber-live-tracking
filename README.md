# Guber Live Tracking Module

## Overview
The **Guber Live Tracking module** provides backend capabilities for real-time location tracking, routing, and fare estimation for the *Guber* ride-sharing app.  
It connects mobile clients (drivers/riders), the navigation/routing engine, and the payment system.

## Features
- Collect driver GPS coordinates (via WebSocket).
- Broadcast live locations to clients.
- Fetch routes & estimated costs between pickup and destination.
- Geocoding (addresses ↔ coordinates).
- Designed to integrate with Payment, User, and Driver modules.

## Endpoints
- `POST /geocode/` → forward or reverse geocoding (uses Nominatim/OSM).
- `POST /route/` → route calculation between A and B (future: OSRM/Mapbox).
- `WS /ws/driver/{driver_id}` → live driver updates.

## Deployment
- Backend: Python FastAPI
- Containerization: Docker (works with Traccar if needed)
- Docs: OpenAPI auto-generated at `/docs`

## Development
- Team, kindly use virtual environment for development to avoid any version issues.
- Always create a new branch for development and issue PR to merge code into main.

- How to run:
  1. Clone the repository:
     ```bash
     git clone <repo-url>
     cd guber-live-tracking
     ```

  2. Create and activate a virtual environment:
     ```bash
     python -m venv .venv
     # On Linux/Mac
     source .venv/bin/activate
     # On Windows
     .venv\Scripts\activate
     ```

  3. Install dependencies:
     ```bash
     pip install -r requirements.txt
     ```

  4. Run the development server:
     ```bash
     uvicorn app.main:app --reload
     ```

- **Branch naming convention:**  
  Use clear prefixes like:
  - `feature/<task>-<short-desc>-<your-name>`  
  - `bugfix/<short-desc>`  
  - `docs/<short-desc>`  

  Example:   
   ``` feature/get-api-lat-long-rishi```