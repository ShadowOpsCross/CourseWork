namespace Courier.Core.Services
{
    /// <summary>Усі тарифні коефіцієнти в одному місці.</summary>
    public static class PricingConfig
    {
        public const double MaxWeightKg = 30.0;
        public const double VolumetricDivisor = 5000.0;
        public const decimal BaseTariff = 50m;
        public const decimal PricePerKg = 12m;
        public const decimal PricePerKm = 0.6m;
        public const decimal ExpressMultiplier = 1.5m;
    }
}
