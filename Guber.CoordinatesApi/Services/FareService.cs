using Guber.CoordinatesApi.Models;

namespace Guber.CoordinatesApi.Services;

public sealed class FareService : IFareService
{
    private readonly double _baseFare;
    private readonly double _perKm;
    public FareService(IConfiguration config)
    {
        _baseFare = config.GetSection("Fare").GetValue<double>("BaseFare", 4.25);
        _perKm   = config.GetSection("Fare").GetValue<double>("PerKm", 1.70);
    }

    public FareResponse Calculate(double distanceKm)
    {
        var total = Math.Round(_baseFare + (distanceKm * _perKm), 2, MidpointRounding.AwayFromZero);
        return new FareResponse(_baseFare, _perKm, distanceKm, total);
    }
}
