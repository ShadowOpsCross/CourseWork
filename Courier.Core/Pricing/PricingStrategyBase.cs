using Courier.Core.Interfaces;
using Courier.Core.Models;
using Courier.Core.Services;

namespace Courier.Core.Pricing
{
    /// <summary>Шаблон розрахунку: (база + вага*ставка + км*ставка) * множник типу доставки.</summary>
    public abstract class PricingStrategyBase : IPricingStrategy
    {
        protected abstract decimal Multiplier { get; }

        public decimal Calculate(Parcel parcel)
        {
            ArgumentNullException.ThrowIfNull(parcel);
            decimal total = PricingConfig.BaseTariff
                + (decimal)parcel.ChargeableWeightKg * PricingConfig.PricePerKg
                + parcel.Route.DistanceKm * PricingConfig.PricePerKm;
            return Math.Round(total * Multiplier, 2);
        }
    }
}
