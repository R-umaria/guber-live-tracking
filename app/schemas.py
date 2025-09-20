"""Pydantic models for request and response bodies."""

from pydantic import BaseModel, Field
from typing import Optional

class LocationUpdate(BaseModel):
    driver_id: str = Field(..., example="driver123")
    latitude: float = Field(..., ge=-90, le=90)
    longitude: float = Field(..., ge=-180, le=180)

class GeocodeRequest(BaseModel):
    query: str

class GeocodeResponse(BaseModel):
    latitude: float
    longitude: float
    display_name: str

class RouteRequest(BaseModel):
    origin_lat: float
    origin_lon: float
    dest_lat: float
    dest_lon: float

class RouteResponse(BaseModel):
    distance_km: float
    duration_min: float
    polyline: Optional[str] = None
