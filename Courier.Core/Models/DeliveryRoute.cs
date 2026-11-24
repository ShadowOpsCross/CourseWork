using System.Text.Json.Serialization;
using Courier.Core.Enums;
using Courier.Core.Exceptions;

namespace Courier.Core.Models
{
    /// <summary>Маршрут доставки: звідки, куди, відстань, тип.</summary>
    public class DeliveryRoute
    {
        private string _fromCity = string.Empty;
        private string _toCity = string.Empty;
        private int _distanceKm = 1;

        public string FromCity
        {
            get => _fromCity;
            set => _fromCity = string.IsNullOrWhiteSpace(value)
                ? throw new InvalidParcelDataException("Місто відправлення не може бути порожнім.")
                : value.Trim();
        }

        public string ToCity
        {
            get => _toCity;
            set => _toCity = string.IsNullOrWhiteSpace(value)
                ? throw new InvalidParcelDataException("Місто призначення не може бути порожнім.")
                : value.Trim();
        }

        public int DistanceKm
        {
            get => _distanceKm;
            set => _distanceKm = value > 0
                ? value
                : throw new InvalidParcelDataException("Відстань має бути більшою за 0.");
        }

        public DeliveryType Type { get; set; }

        /// <summary>Орієнтовний термін доставки в днях.</summary>
        [JsonIgnore]
        public int EstimatedDays => Type == DeliveryType.Express
            ? (int)Math.Ceiling(DistanceKm / 800.0)
            : (int)Math.Ceiling(DistanceKm / 400.0) + 1;

        public override string ToString() => $"{FromCity} -> {ToCity}, {DistanceKm} км, {Type}";
    }
}
