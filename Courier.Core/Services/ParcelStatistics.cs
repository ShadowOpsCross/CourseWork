using Courier.Core.Enums;

namespace Courier.Core.Services
{
    /// <summary>Зведена статистика за всіма посилками.</summary>
    public class ParcelStatistics
    {
        public int TotalCount { get; init; }
        public decimal TotalCost { get; init; }
        public double AverageWeightKg { get; init; }
        public Dictionary<ParcelStatus, int> CountByStatus { get; init; } = new();
    }
}
