using System.Collections.Generic;
using Guber.CoordinatesApi.Models;

namespace Guber.CoordinatesApi.Services;

/// <summary>
/// Decodes an encoded polyline (Google/OSRM) into an ordered list of lat/lon points.
/// Supports precision 5 (1e5) and 6 (1e6). Use 6 for OSRM "polyline6".
/// </summary>
internal static class PolylineDecoder
{
    public static IReadOnlyList<CoordinatePoint> Decode(string? encoded, int precision = 5)
    {
        var result = new List<CoordinatePoint>();
        if (string.IsNullOrWhiteSpace(encoded))
            return result;

        int index = 0, lat = 0, lng = 0;
        double factor = Math.Pow(10, precision);

        while (index < encoded.Length)
        {
            int b, shift = 0, accum = 0;
            do
            {
                b = encoded[index++] - 63;
                accum |= (b & 0x1f) << shift;
                shift += 5;
            } while (b >= 0x20 && index < encoded.Length);
            int deltaLat = ((accum & 1) != 0) ? ~(accum >> 1) : (accum >> 1);
            lat += deltaLat;

            shift = 0; accum = 0;
            do
            {
                b = encoded[index++] - 63;
                accum |= (b & 0x1f) << shift;
                shift += 5;
            } while (b >= 0x20 && index < encoded.Length);
            int deltaLng = ((accum & 1) != 0) ? ~(accum >> 1) : (accum >> 1);
            lng += deltaLng;

            result.Add(new CoordinatePoint(
                Lat: lat / factor,
                Lon: lng / factor
            ));
        }

        return result;
    }
}
