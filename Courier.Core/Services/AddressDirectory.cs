using Courier.Core.Exceptions;
using Courier.Core.Interfaces;

namespace Courier.Core.Services
{
    /// <summary>Довідник міст України з орієнтовними автомобільними відстанями (км).</summary>
    public class AddressDirectory : IAddressDirectory
    {
        private const int IntraCityDistanceKm = 15;

        private readonly Dictionary<(string, string), int> _distances =
            new(new CityPairComparer());

        private readonly List<string> _cities = new();

        public AddressDirectory()
        {
            AddRoad("Київ", "Львів", 540);
            AddRoad("Київ", "Одеса", 475);
            AddRoad("Київ", "Харків", 480);
            AddRoad("Київ", "Дніпро", 480);
            AddRoad("Київ", "Вінниця", 270);
            AddRoad("Київ", "Полтава", 340);
            AddRoad("Київ", "Чернівці", 530);
            AddRoad("Львів", "Одеса", 790);
            AddRoad("Львів", "Харків", 1020);
            AddRoad("Львів", "Дніпро", 970);
            AddRoad("Львів", "Вінниця", 380);
            AddRoad("Львів", "Полтава", 880);
            AddRoad("Львів", "Чернівці", 270);
            AddRoad("Одеса", "Харків", 700);
            AddRoad("Одеса", "Дніпро", 480);
            AddRoad("Одеса", "Вінниця", 420);
            AddRoad("Одеса", "Полтава", 570);
            AddRoad("Одеса", "Чернівці", 560);
            AddRoad("Харків", "Дніпро", 215);
            AddRoad("Харків", "Вінниця", 750);
            AddRoad("Харків", "Полтава", 145);
            AddRoad("Харків", "Чернівці", 950);
            AddRoad("Дніпро", "Вінниця", 600);
            AddRoad("Дніпро", "Полтава", 195);
            AddRoad("Дніпро", "Чернівці", 840);
            AddRoad("Вінниця", "Полтава", 600);
            AddRoad("Вінниця", "Чернівці", 260);
            AddRoad("Полтава", "Чернівці", 810);
        }

        public IReadOnlyCollection<string> Cities => _cities;

        public bool Contains(string city) =>
            !string.IsNullOrWhiteSpace(city) &&
            _cities.Contains(city.Trim(), StringComparer.OrdinalIgnoreCase);

        public int GetDistanceKm(string fromCity, string toCity)
        {
            if (!Contains(fromCity)) throw new AddressNotFoundException(fromCity);
            if (!Contains(toCity)) throw new AddressNotFoundException(toCity);
            if (string.Equals(fromCity.Trim(), toCity.Trim(), StringComparison.OrdinalIgnoreCase))
                return IntraCityDistanceKm;
            return _distances[(fromCity.Trim(), toCity.Trim())];
        }

        private void AddRoad(string a, string b, int km)
        {
            if (!_cities.Contains(a)) _cities.Add(a);
            if (!_cities.Contains(b)) _cities.Add(b);
            _distances[(a, b)] = km;
        }

        /// <summary>Порівнює пару міст без урахування регістру та порядку.</summary>
        private sealed class CityPairComparer : IEqualityComparer<(string, string)>
        {
            public bool Equals((string, string) x, (string, string) y) =>
                (Same(x.Item1, y.Item1) && Same(x.Item2, y.Item2)) ||
                (Same(x.Item1, y.Item2) && Same(x.Item2, y.Item1));

            public int GetHashCode((string, string) obj) =>
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item1) ^
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item2);

            private static bool Same(string a, string b) =>
                string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}
