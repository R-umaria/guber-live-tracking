using Guber.CoordinatesApi.Models;

namespace Guber.CoordinatesApi.Services;

public sealed class FareService : IFareService
{
    private readonly double _baseFare;
    private readonly double _perKm;
    private readonly double _XLMultiplier;
    private readonly double _PetFee;
    public FareService(IConfiguration config)
    {
        _baseFare = config.GetSection("Fare").GetValue<double>("BaseFare", 4.25);
        _perKm   = config.GetSection("Fare").GetValue<double>("PerKm", 1.70);
        _XLMultiplier = config.GetSection("Fare").GetValue<double>("XLMultiplier", .35);
        _PetFee = config.GetSection("Fare").GetValue<double>("PetFee", 7.5);
    }

    public FareResponse Calculate(double distanceKm, string type, bool pet)
    {
        double BaseFare = _baseFare;
        double PerKm = _perKm;
        if (type == "XL")
        {
            PerKm += _XLMultiplier;
        }
        var total = Math.Round(_baseFare + (distanceKm * PerKm), 2, MidpointRounding.AwayFromZero);
        if(pet)
        {
            total += _PetFee;
        }
        return new FareResponse(_baseFare, PerKm, _XLMultiplier ,_PetFee, distanceKm, total);
    }
}
