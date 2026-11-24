using Courier.Core.Models;

namespace Courier.Core.Interfaces
{
    /// <summary>Стратегія розрахунку вартості доставки.</summary>
    public interface IPricingStrategy
    {
        decimal Calculate(Parcel parcel);
    }
}
