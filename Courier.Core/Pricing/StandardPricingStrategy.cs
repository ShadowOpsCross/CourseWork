namespace Courier.Core.Pricing
{
    /// <summary>Стандартна доставка (множник 1).</summary>
    public class StandardPricingStrategy : PricingStrategyBase
    {
        protected override decimal Multiplier => 1m;
    }
}
