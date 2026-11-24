using Courier.Core.Models;
using Courier.Core.Services;

namespace Courier.UI
{
    /// <summary>Форматований вивід у консоль.</summary>
    public static class ConsolePrinter
    {
        public static void Error(string message) => WriteColored($"  [!] {message}", ConsoleColor.Red);

        public static void Success(string message) => WriteColored($"  [OK] {message}", ConsoleColor.Green);

        public static void Info(string message) => WriteColored($"  {message}", ConsoleColor.Cyan);

        public static void Header(string title)
        {
            Console.WriteLine();
            WriteColored($"=== {title} ===", ConsoleColor.Yellow);
        }

        public static void PrintTable(IReadOnlyList<Parcel> parcels)
        {
            if (parcels.Count == 0)
            {
                Info("Нічого не знайдено.");
                return;
            }

            Console.WriteLine($"{"Трекінг",-11}{"Опис",-18}{"Вага,кг",8} {"Маршрут",-20}{"Тип",-9}{"Статус",-15}{"Вартість",10}");
            Console.WriteLine(new string('-', 93));
            foreach (Parcel p in parcels)
            {
                string description = p.Description.Length > 16 ? p.Description[..15] + "…" : p.Description;
                string route = $"{p.Route.FromCity}->{p.Route.ToCity}";
                Console.WriteLine(
                    $"{p.TrackingNumber,-11}{description,-18}{p.WeightKg,8:F2} {route,-20}{p.Route.Type,-9}{p.Status,-15}{p.Cost,10:F2}");
            }
            Console.WriteLine(new string('-', 93));
            Console.WriteLine($"Всього: {parcels.Count}");
        }

        public static void PrintDetails(Parcel p)
        {
            Console.WriteLine($"Трекінг-номер : {p.TrackingNumber}");
            Console.WriteLine($"Опис          : {p.Description}");
            Console.WriteLine($"Відправник    : {p.Sender}");
            Console.WriteLine($"Одержувач     : {p.Recipient.FullName} ({p.Recipient.Phone})");
            Console.WriteLine($"Адреса        : {p.Recipient.FullAddress}");
            Console.WriteLine($"Маршрут       : {p.Route}");
            Console.WriteLine($"Вага          : {p.WeightKg:F2} кг (об'ємна {p.VolumetricWeightKg:F2} кг, розрахункова {p.ChargeableWeightKg:F2} кг)");
            Console.WriteLine($"Габарити      : {p.LengthCm}x{p.WidthCm}x{p.HeightCm} см");
            Console.WriteLine($"Термін        : ~{p.Route.EstimatedDays} дн.");
            Console.WriteLine($"Статус        : {p.Status}");
            Console.WriteLine($"Вартість      : {p.Cost:F2} грн");
        }

        public static void PrintHistory(Parcel p)
        {
            foreach (StatusRecord record in p.History)
                Console.WriteLine($"  {record.Timestamp:dd.MM.yyyy HH:mm:ss}  {record.Status}");
        }

        public static void PrintStatistics(ParcelStatistics s)
        {
            Console.WriteLine($"Загальна кількість посилок : {s.TotalCount}");
            Console.WriteLine($"Сумарна вартість           : {s.TotalCost:F2} грн");
            Console.WriteLine($"Середня вага               : {s.AverageWeightKg:F2} кг");
            Console.WriteLine("За статусами:");
            foreach (var pair in s.CountByStatus)
                Console.WriteLine($"  {pair.Key,-15}: {pair.Value}");
        }

        private static void WriteColored(string text, ConsoleColor color)
        {
            ConsoleColor previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = previous;
        }
    }
}
