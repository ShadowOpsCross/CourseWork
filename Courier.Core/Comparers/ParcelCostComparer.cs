using Courier.Core.Models;

namespace Courier.Core.Comparers
{
    /// <summary>Порівняння посилок за вартістю доставки.</summary>
    public class ParcelCostComparer : IComparer<Parcel>
    {
        public int Compare(Parcel? x, Parcel? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.Cost.CompareTo(y.Cost);
        }
    }
}
