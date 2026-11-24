namespace Courier.Core.Interfaces
{
    /// <summary>Довідник населених пунктів та відстаней між ними.</summary>
    public interface IAddressDirectory
    {
        IReadOnlyCollection<string> Cities { get; }
        bool Contains(string city);
        /// <summary>Повертає відстань у км; кидає AddressNotFoundException, якщо місто невідоме.</summary>
        int GetDistanceKm(string fromCity, string toCity);
    }
}
