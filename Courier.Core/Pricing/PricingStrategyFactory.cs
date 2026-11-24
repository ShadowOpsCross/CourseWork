using Courier.Core.Enums;
using Courier.Core.Interfaces;

namespace Courier.Core.Pricing
{
    /// <summary>Підбирає стратегію розрахунку за типом доставки.</summary>
    public static class PricingStrategyFactory
    {
        public static IPricingStrategy Create(DeliveryType type) => type switch
        {
            DeliveryType.Express => new ExpressPricingStrategy(),
            _ => new StandardPricingStrategy()
        };
    }
}
