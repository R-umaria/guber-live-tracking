"""FastAPI entry point for Guber live tracking service."""

from fastapi import FastAPI, WebSocket
from .schemas import LocationUpdate, GeocodeRequest, GeocodeResponse, RouteRequest, RouteResponse
import httpx

app = FastAPI(title="Guber Live Tracking")

# in-memory storage for demo
drivers = {}

@app.websocket("/ws/driver/{driver_id}")
async def driver_ws(websocket: WebSocket, driver_id: str):
    await websocket.accept()
    while True:
        data = await websocket.receive_json()
        update = LocationUpdate(**data)
        drivers[driver_id] = (update.latitude, update.longitude)
        await websocket.send_json({"status": "ok", "driver": driver_id})

@app.post("/geocode/", response_model=GeocodeResponse)
async def geocode(req: GeocodeRequest):
    url = f"https://nominatim.openstreetmap.org/search"
    params = {"q": req.query, "format": "json", "limit": 1}
    async with httpx.AsyncClient() as client:
        r = await client.get(url, params=params)
        r.raise_for_status()
        result = r.json()[0]
    return GeocodeResponse(
        latitude=float(result["lat"]),
        longitude=float(result["lon"]),
        display_name=result["display_name"],
    )

@app.post("/route/", response_model=RouteResponse)
async def route(req: RouteRequest):
    # placeholder: straight-line distance
    from math import sqrt
    dx = req.origin_lat - req.dest_lat
    dy = req.origin_lon - req.dest_lon
    dist = sqrt(dx*dx + dy*dy) * 111  # rough km conversion
    return RouteResponse(distance_km=dist, duration_min=dist/0.5)
