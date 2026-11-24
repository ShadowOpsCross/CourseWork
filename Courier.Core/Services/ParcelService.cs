using Courier.Core.Enums;
using Courier.Core.Exceptions;
using Courier.Core.Interfaces;
using Courier.Core.Models;
using Courier.Core.Pricing;
using Courier.Core.Repositories;

namespace Courier.Core.Services
{
    /// <summary>Бізнес-логіка служби доставки: створення, пошук, статуси, статистика.</summary>
    public class ParcelService
    {
        private readonly ParcelRepository _repository;
        private readonly IAddressDirectory _directory;

        public ParcelService(ParcelRepository repository, IAddressDirectory directory)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        public IAddressDirectory Directory => _directory;

        /// <summary>Створює посилку, будує маршрут і розраховує вартість.</summary>
        public Parcel CreateParcel(Sender sender, Recipient recipient, string description,
            double weightKg, double lengthCm, double widthCm, double heightCm, DeliveryType type)
        {
            Parcel parcel = new()
            {
                Sender = sender,
                Recipient = recipient,
                Description = description,
                WeightKg = weightKg,
                LengthCm = lengthCm,
                WidthCm = widthCm,
                HeightCm = heightCm,
                Route = BuildRoute(sender.City, recipient.City, type)
            };
            parcel.SetCost(PricingStrategyFactory.Create(type).Calculate(parcel));
            _repository.Add(parcel);
            return parcel;
        }

        /// <summary>Попередній розрахунок вартості без створення посилки.</summary>
        public decimal EstimateCost(string fromCity, string toCity, double weightKg,
            double lengthCm, double widthCm, double heightCm, DeliveryType type)
        {
            Parcel draft = new()
            {
                WeightKg = weightKg,
                LengthCm = lengthCm,
                WidthCm = widthCm,
                HeightCm = heightCm,
                Route = BuildRoute(fromCity, toCity, type)
            };
            return PricingStrategyFactory.Create(type).Calculate(draft);
        }

        public DeliveryRoute BuildRoute(string fromCity, string toCity, DeliveryType type) => new()
        {
            FromCity = fromCity,
            ToCity = toCity,
            DistanceKm = _directory.GetDistanceKm(fromCity, toCity),
            Type = type
        };

        /// <summary>Редагування можливе лише поки посилка в статусі Created; вартість перераховується.</summary>
        public Parcel EditParcel(string trackingNumber, string description, double weightKg,
            double lengthCm, double widthCm, double heightCm)
        {
            Parcel parcel = _repository.GetByTrackingNumber(trackingNumber);
            if (parcel.Status != ParcelStatus.Created)
                throw new InvalidParcelDataException(
                    "Редагувати можна лише посилки зі статусом Created.");
            parcel.Description = description;
            parcel.WeightKg = weightKg;
            parcel.LengthCm = lengthCm;
            parcel.WidthCm = widthCm;
            parcel.HeightCm = heightCm;
            parcel.SetCost(PricingStrategyFactory.Create(parcel.Route.Type).Calculate(parcel));
            return parcel;
        }

        public void ChangeStatus(string trackingNumber, ParcelStatus newStatus) =>
            _repository.GetByTrackingNumber(trackingNumber).ChangeStatus(newStatus);

        public Parcel GetByTrackingNumber(string trackingNumber) =>
            _repository.GetByTrackingNumber(trackingNumber);

        public void DeleteParcel(string trackingNumber)
        {
            Parcel parcel = _repository.GetByTrackingNumber(trackingNumber);
            _repository.Remove(parcel);
        }

        public List<Parcel> GetAll() => _repository.GetAll();

        // ---------- Пошук і фільтрація ----------

        public List<Parcel> FindByStatus(ParcelStatus status) =>
            _repository.Find(p => p.Status == status);

        public List<Parcel> FindByPersonName(string namePart)
        {
            if (string.IsNullOrWhiteSpace(namePart))
                throw new InvalidParcelDataException("Введіть частину імені для пошуку.");
            string needle = namePart.Trim();
            return _repository.Find(p =>
                p.Sender.FullName.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                p.Recipient.FullName.Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        public List<Parcel> FindByWeightRange(double minKg, double maxKg)
        {
            if (minKg < 0 || maxKg < minKg)
                throw new InvalidParcelDataException("Некоректний діапазон ваги.");
            return _repository.Find(p => p.WeightKg >= minKg && p.WeightKg <= maxKg);
        }

        public List<Parcel> FindByCostRange(decimal minCost, decimal maxCost)
        {
            if (minCost < 0 || maxCost < minCost)
                throw new InvalidParcelDataException("Некоректний діапазон вартості.");
            return _repository.Find(p => p.Cost >= minCost && p.Cost <= maxCost);
        }

        public List<Parcel> FindByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new InvalidParcelDataException("Введіть назву міста.");
            string c = city.Trim();
            return _repository.Find(p =>
                string.Equals(p.Route.FromCity, c, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Route.ToCity, c, StringComparison.OrdinalIgnoreCase));
        }

        // ---------- Сортування ----------

        public List<Parcel> GetSorted(IComparer<Parcel> comparer, bool descending = false)
        {
            ArgumentNullException.ThrowIfNull(comparer);
            List<Parcel> list = _repository.GetAll();
            list.Sort(comparer);
            if (descending) list.Reverse();
            return list;
        }

        // ---------- Статистика ----------

        public ParcelStatistics GetStatistics()
        {
            List<Parcel> all = _repository.GetAll();
            Dictionary<ParcelStatus, int> byStatus = Enum.GetValues<ParcelStatus>()
                .ToDictionary(s => s, s => all.Count(p => p.Status == s));

            return new ParcelStatistics
            {
                TotalCount = all.Count,
                TotalCost = all.Sum(p => p.Cost),
                AverageWeightKg = all.Count == 0 ? 0 : all.Average(p => p.WeightKg),
                CountByStatus = byStatus
            };
        }
    }
}
