using System.Text.Json.Serialization;
using Courier.Core.Enums;
using Courier.Core.Exceptions;
using Courier.Core.Services;

namespace Courier.Core.Models
{
    /// <summary>Посилка. Статус змінюється лише через метод ChangeStatus.</summary>
    public class Parcel : IComparable<Parcel>
    {
        private string _description = string.Empty;
        private double _weightKg = 0.1;
        private double _lengthCm = 1;
        private double _widthCm = 1;
        private double _heightCm = 1;
        private Sender _sender = new();
        private Recipient _recipient = new();
        private DeliveryRoute _route = new();

        public string TrackingNumber { get; set; } =
            "CR" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        public Sender Sender
        {
            get => _sender;
            set => _sender = value ?? throw new InvalidParcelDataException("Відправник обов'язковий.");
        }

        public Recipient Recipient
        {
            get => _recipient;
            set => _recipient = value ?? throw new InvalidParcelDataException("Одержувач обов'язковий.");
        }

        public DeliveryRoute Route
        {
            get => _route;
            set => _route = value ?? throw new InvalidParcelDataException("Маршрут обов'язковий.");
        }

        public string Description
        {
            get => _description;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 200)
                    throw new InvalidParcelDataException("Опис має містити від 1 до 200 символів.");
                _description = value.Trim();
            }
        }

        public double WeightKg
        {
            get => _weightKg;
            set
            {
                if (double.IsNaN(value) || value <= 0)
                    throw new InvalidParcelDataException("Вага має бути більшою за 0.");
                if (value > PricingConfig.MaxWeightKg)
                    throw new WeightLimitExceededException(value, PricingConfig.MaxWeightKg);
                _weightKg = value;
            }
        }

        public double LengthCm { get => _lengthCm; set => _lengthCm = ValidateDimension(value, "Довжина"); }
        public double WidthCm { get => _widthCm; set => _widthCm = ValidateDimension(value, "Ширина"); }
        public double HeightCm { get => _heightCm; set => _heightCm = ValidateDimension(value, "Висота"); }

        [JsonInclude]
        public ParcelStatus Status { get; private set; } = ParcelStatus.Created;

        [JsonInclude]
        public decimal Cost { get; private set; }

        [JsonInclude]
        public DateTime CreatedAt { get; private set; } = DateTime.Now;

        [JsonInclude]
        public List<StatusRecord> History { get; private set; } =
            new() { new StatusRecord(ParcelStatus.Created, DateTime.Now) };

        /// <summary>Об'єм у см³.</summary>
        [JsonIgnore]
        public double VolumeCm3 => LengthCm * WidthCm * HeightCm;

        /// <summary>Об'ємна вага (кг).</summary>
        [JsonIgnore]
        public double VolumetricWeightKg => VolumeCm3 / PricingConfig.VolumetricDivisor;

        /// <summary>Розрахункова вага — більша з фактичної та об'ємної.</summary>
        [JsonIgnore]
        public double ChargeableWeightKg => Math.Max(WeightKg, VolumetricWeightKg);

        public void SetCost(decimal cost)
        {
            if (cost < 0) throw new InvalidParcelDataException("Вартість не може бути від'ємною.");
            Cost = cost;
        }

        /// <summary>Змінює статус, перевіряючи допустимість переходу.</summary>
        public void ChangeStatus(ParcelStatus newStatus)
        {
            if (!StatusTransitions.IsAllowed(Status, newStatus))
                throw new InvalidStatusTransitionException(Status, newStatus);
            Status = newStatus;
            History.Add(new StatusRecord(newStatus, DateTime.Now));
        }

        public int CompareTo(Parcel? other)
        {
            if (other == null) return 1;
            int byDate = CreatedAt.CompareTo(other.CreatedAt);
            return byDate != 0 ? byDate : string.CompareOrdinal(TrackingNumber, other.TrackingNumber);
        }

        private static double ValidateDimension(double value, string name)
        {
            if (double.IsNaN(value) || value <= 0 || value > 150)
                throw new InvalidParcelDataException($"{name} має бути в межах (0; 150] см.");
            return value;
        }

        public override string ToString() => $"{TrackingNumber} | {Description} | {WeightKg} кг | {Status}";
    }
}
