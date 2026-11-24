using Courier.Core.Models;

namespace Courier.Core.Comparers
{
    /// <summary>Порівняння посилок за статусом (у порядку життєвого циклу).</summary>
    public class ParcelStatusComparer : IComparer<Parcel>
    {
        public int Compare(Parcel? x, Parcel? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.Status.CompareTo(y.Status);
        }
    }
}
