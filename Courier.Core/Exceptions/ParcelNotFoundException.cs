namespace Courier.Core.Exceptions
{
    /// <summary>Посилку з вказаним трекінг-номером не знайдено.</summary>
    public class ParcelNotFoundException : Exception
    {
        public string TrackingNumber { get; }

        public ParcelNotFoundException(string trackingNumber)
            : base($"Посилку з номером \"{trackingNumber}\" не знайдено.")
        {
            TrackingNumber = trackingNumber;
        }
    }
}
