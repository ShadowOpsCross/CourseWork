using Courier.Core.Enums;

namespace Courier.Core.Services
{
    /// <summary>Таблиця допустимих переходів між статусами.</summary>
    public static class StatusTransitions
    {
        private static readonly Dictionary<ParcelStatus, ParcelStatus[]> Allowed = new()
        {
            [ParcelStatus.Created] = new[] { ParcelStatus.Accepted },
            [ParcelStatus.Accepted] = new[] { ParcelStatus.InTransit },
            [ParcelStatus.InTransit] = new[] { ParcelStatus.OutForDelivery, ParcelStatus.Returned },
            [ParcelStatus.OutForDelivery] = new[] { ParcelStatus.Delivered, ParcelStatus.Returned },
            [ParcelStatus.Delivered] = Array.Empty<ParcelStatus>(),
            [ParcelStatus.Returned] = Array.Empty<ParcelStatus>()
        };

        public static bool IsAllowed(ParcelStatus from, ParcelStatus to) => Allowed[from].Contains(to);

        public static IReadOnlyList<ParcelStatus> GetNext(ParcelStatus from) => Allowed[from];
    }
}
