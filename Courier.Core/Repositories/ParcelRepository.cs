using Courier.Core.Exceptions;
using Courier.Core.Interfaces;
using Courier.Core.Models;

namespace Courier.Core.Repositories
{
    /// <summary>Сховище посилок у пам'яті на основі узагальненої колекції List&lt;T&gt;.</summary>
    public class ParcelRepository : IRepository<Parcel>
    {
        private readonly List<Parcel> _parcels = new();

        public int Count => _parcels.Count;

        public void Add(Parcel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            if (_parcels.Any(p => p.TrackingNumber == item.TrackingNumber))
                throw new InvalidParcelDataException(
                    $"Посилка з номером {item.TrackingNumber} вже існує.");
            _parcels.Add(item);
        }

        public void AddRange(IEnumerable<Parcel> items)
        {
            foreach (Parcel item in items) Add(item);
        }

        public bool Remove(Parcel item) => _parcels.Remove(item);

        /// <summary>Повертає копію списку, щоб зовнішній код не змінював колекцію напряму.</summary>
        public List<Parcel> GetAll() => new(_parcels);

        public List<Parcel> Find(Predicate<Parcel> match) => _parcels.FindAll(match);

        public Parcel GetByTrackingNumber(string trackingNumber)
        {
            Parcel? parcel = _parcels.Find(p =>
                string.Equals(p.TrackingNumber, trackingNumber?.Trim(), StringComparison.OrdinalIgnoreCase));
            return parcel ?? throw new ParcelNotFoundException(trackingNumber ?? string.Empty);
        }
    }
}
