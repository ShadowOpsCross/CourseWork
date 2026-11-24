using Courier.Core.Services;

namespace Courier.Core.Pricing
{
    /// <summary>Експрес-доставка (підвищений множник).</summary>
    public class ExpressPricingStrategy : PricingStrategyBase
    {
        protected override decimal Multiplier => PricingConfig.ExpressMultiplier;
    }
}
