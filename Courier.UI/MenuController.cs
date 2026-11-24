using Courier.Core.Comparers;
using Courier.Core.Enums;
using Courier.Core.Exceptions;
using Courier.Core.Models;
using Courier.Core.Services;

namespace Courier.UI
{
    /// <summary>Головне меню консольного застосування.</summary>
    public class MenuController
    {
        private readonly ParcelService _service;

        public MenuController(ParcelService service)
        {
            _service = service;
        }

        public void Run()
        {
            while (true)
            {
                PrintMenu();
                int choice = InputReader.ReadInt("Оберіть пункт: ", 0, 11);
                if (choice == 0) return;

                try
                {
                    Execute(choice);
                }
                catch (InputEndedException)
                {
                    throw;
                }
                catch (WeightLimitExceededException ex)
                {
                    ConsolePrinter.Error($"[WeightLimitExceeded] {ex.Message}");
                }
                catch (AddressNotFoundException ex)
                {
                    ConsolePrinter.Error($"[AddressNotFound] {ex.Message} Доступні міста: {string.Join(", ", _service.Directory.Cities)}.");
                }
                catch (ParcelNotFoundException ex)
                {
                    ConsolePrinter.Error($"[ParcelNotFound] {ex.Message}");
                }
                catch (InvalidStatusTransitionException ex)
                {
                    ConsolePrinter.Error($"[InvalidStatusTransition] {ex.Message}");
                }
                catch (InvalidParcelDataException ex)
                {
                    ConsolePrinter.Error($"[InvalidData] {ex.Message}");
                }
                catch (Exception ex)
                {
                    ConsolePrinter.Error($"Непередбачена помилка: {ex.Message}");
                }
            }
        }

        private static void PrintMenu()
        {
            ConsolePrinter.Header("СЛУЖБА КУР'ЄРСЬКОЇ ДОСТАВКИ");
            Console.WriteLine(" 1. Додати посилку");
            Console.WriteLine(" 2. Розрахувати вартість (без створення)");
            Console.WriteLine(" 3. Змінити статус посилки");
            Console.WriteLine(" 4. Редагувати посилку");
            Console.WriteLine(" 5. Видалити посилку");
            Console.WriteLine(" 6. Показати всі посилки");
            Console.WriteLine(" 7. Пошук (номер / ім'я / місто / вага / вартість)");
            Console.WriteLine(" 8. Фільтр за статусом");
            Console.WriteLine(" 9. Сортування");
            Console.WriteLine("10. Статистика");
            Console.WriteLine("11. Історія та деталі посилки");
            Console.WriteLine(" 0. Вихід (зі збереженням)");
        }

        private void Execute(int choice)
        {
            switch (choice)
            {
                case 1: AddParcel(); break;
                case 2: EstimateCost(); break;
                case 3: ChangeStatus(); break;
                case 4: EditParcel(); break;
                case 5: DeleteParcel(); break;
                case 6: ShowAll(); break;
                case 7: Search(); break;
                case 8: FilterByStatus(); break;
                case 9: Sort(); break;
                case 10: ShowStatistics(); break;
                case 11: ShowDetails(); break;
            }
        }

        // ---------- Пункти меню ----------

        private void AddParcel()
        {
            ConsolePrinter.Header("Нова посилка");
            Sender sender = ReadSender();
            Recipient recipient = ReadRecipient();

            string description = ReadDescription("Опис вмісту: ");
            double weight = InputReader.ReadDouble("Вага, кг: ", 0.01, 1000);
            double length = InputReader.ReadDouble("Довжина, см: ", 0.1, 1000);
            double width = InputReader.ReadDouble("Ширина, см: ", 0.1, 1000);
            double height = InputReader.ReadDouble("Висота, см: ", 0.1, 1000);
            DeliveryType type = InputReader.ReadEnum("Тип доставки:", Enum.GetValues<DeliveryType>());

            Parcel parcel = _service.CreateParcel(sender, recipient, description, weight, length, width, height, type);
            ConsolePrinter.Success($"Посилку створено. Трекінг-номер: {parcel.TrackingNumber}, вартість: {parcel.Cost:F2} грн.");
        }

        /// <summary>Читає опис, перевіряючи його правилами моделі Parcel.</summary>
        private static string ReadDescription(string prompt)
        {
            string result = string.Empty;
            InputReader.ReadInto(prompt, v =>
            {
                Parcel probe = new() { Description = v };
                result = probe.Description;
            });
            return result;
        }

        private Sender ReadSender()
        {
            List<Sender> known = _service.GetAll()
                .Select(p => p.Sender)
                .GroupBy(s => s.Phone)
                .Select(g => g.First())
                .ToList();

            if (known.Count > 0 && InputReader.ReadYesNo("Обрати існуючого відправника?"))
            {
                for (int i = 0; i < known.Count; i++)
                    Console.WriteLine($"  {i + 1}. {known[i]}");
                return known[InputReader.ReadInt("Номер відправника: ", 1, known.Count) - 1];
            }

            Console.WriteLine("-- Відправник --");
            Sender sender = new();
            InputReader.ReadInto("ПІБ: ", v => sender.FullName = v);
            InputReader.ReadInto("Телефон: ", v => sender.Phone = v);
            InputReader.ReadInto("Місто відправлення: ", v => sender.City = v);
            InputReader.ReadInto("Email (Enter — пропустити): ", v => sender.Email = v, allowEmpty: true);
            return sender;
        }

        private static Recipient ReadRecipient()
        {
            Console.WriteLine("-- Одержувач --");
            Recipient recipient = new();
            InputReader.ReadInto("ПІБ: ", v => recipient.FullName = v);
            InputReader.ReadInto("Телефон: ", v => recipient.Phone = v);
            InputReader.ReadInto("Місто: ", v => recipient.City = v);
            InputReader.ReadInto("Вулиця: ", v => recipient.Street = v);
            InputReader.ReadInto("Будинок: ", v => recipient.Building = v);
            return recipient;
        }

        private void EstimateCost()
        {
            ConsolePrinter.Header("Розрахунок вартості");
            string from = InputReader.ReadLine("Місто відправлення: ");
            string to = InputReader.ReadLine("Місто призначення: ");
            double weight = InputReader.ReadDouble("Вага, кг: ", 0.01, 1000);
            double length = InputReader.ReadDouble("Довжина, см: ", 0.1, 1000);
            double width = InputReader.ReadDouble("Ширина, см: ", 0.1, 1000);
            double height = InputReader.ReadDouble("Висота, см: ", 0.1, 1000);
            DeliveryType type = InputReader.ReadEnum("Тип доставки:", Enum.GetValues<DeliveryType>());

            decimal cost = _service.EstimateCost(from, to, weight, length, width, height, type);
            ConsolePrinter.Success($"Орієнтовна вартість: {cost:F2} грн.");
        }

        private void ChangeStatus()
        {
            ConsolePrinter.Header("Зміна статусу");
            Parcel parcel = _service.GetByTrackingNumber(InputReader.ReadLine("Трекінг-номер: "));
            Console.WriteLine($"Поточний статус: {parcel.Status}");

            ParcelStatus newStatus = InputReader.ReadEnum("Новий статус:", Enum.GetValues<ParcelStatus>());
            _service.ChangeStatus(parcel.TrackingNumber, newStatus);
            ConsolePrinter.Success($"Статус змінено на {newStatus}.");
        }

        private void EditParcel()
        {
            ConsolePrinter.Header("Редагування посилки");
            Parcel parcel = _service.GetByTrackingNumber(InputReader.ReadLine("Трекінг-номер: "));
            string description = ReadDescription("Новий опис: ");
            double weight = InputReader.ReadDouble("Нова вага, кг: ", 0.01, 1000);
            double length = InputReader.ReadDouble("Довжина, см: ", 0.1, 1000);
            double width = InputReader.ReadDouble("Ширина, см: ", 0.1, 1000);
            double height = InputReader.ReadDouble("Висота, см: ", 0.1, 1000);

            Parcel edited = _service.EditParcel(parcel.TrackingNumber, description, weight, length, width, height);
            ConsolePrinter.Success($"Посилку оновлено. Нова вартість: {edited.Cost:F2} грн.");
        }

        private void DeleteParcel()
        {
            ConsolePrinter.Header("Видалення посилки");
            string tracking = InputReader.ReadLine("Трекінг-номер: ");
            Parcel parcel = _service.GetByTrackingNumber(tracking);
            if (InputReader.ReadYesNo($"Видалити {parcel.TrackingNumber}?"))
            {
                _service.DeleteParcel(parcel.TrackingNumber);
                ConsolePrinter.Success("Посилку видалено.");
            }
        }

        private void ShowAll()
        {
            ConsolePrinter.Header("Усі посилки");
            ConsolePrinter.PrintTable(_service.GetAll());
        }

        private void Search()
        {
            ConsolePrinter.Header("Пошук");
            Console.WriteLine("1. За трекінг-номером\n2. За іменем відправника/одержувача\n3. За містом\n4. За діапазоном ваги\n5. За діапазоном вартості");
            int mode = InputReader.ReadInt("Критерій: ", 1, 5);

            List<Parcel> result;
            switch (mode)
            {
                case 1:
                    result = new List<Parcel> { _service.GetByTrackingNumber(InputReader.ReadLine("Трекінг-номер: ")) };
                    break;
                case 2:
                    result = _service.FindByPersonName(InputReader.ReadLine("Частина імені: "));
                    break;
                case 3:
                    result = _service.FindByCity(InputReader.ReadLine("Місто: "));
                    break;
                case 4:
                    result = _service.FindByWeightRange(
                        InputReader.ReadDouble("Вага від, кг: ", 0, 1000),
                        InputReader.ReadDouble("Вага до, кг: ", 0, 1000));
                    break;
                default:
                    result = _service.FindByCostRange(
                        InputReader.ReadDecimal("Вартість від: ", 0, 1_000_000),
                        InputReader.ReadDecimal("Вартість до: ", 0, 1_000_000));
                    break;
            }
            ConsolePrinter.PrintTable(result);
        }

        private void FilterByStatus()
        {
            ConsolePrinter.Header("Фільтр за статусом");
            ParcelStatus status = InputReader.ReadEnum("Статус:", Enum.GetValues<ParcelStatus>());
            ConsolePrinter.PrintTable(_service.FindByStatus(status));
        }

        private void Sort()
        {
            ConsolePrinter.Header("Сортування");
            Console.WriteLine("1. За вагою\n2. За вартістю\n3. За датою створення\n4. За статусом");
            int mode = InputReader.ReadInt("Критерій: ", 1, 4);
            bool descending = InputReader.ReadYesNo("Від більшого до меншого?");

            IComparer<Parcel> comparer = mode switch
            {
                1 => new ParcelWeightComparer(),
                2 => new ParcelCostComparer(),
                3 => new ParcelDateComparer(),
                _ => new ParcelStatusComparer()
            };
            ConsolePrinter.PrintTable(_service.GetSorted(comparer, descending));
        }

        private void ShowStatistics()
        {
            ConsolePrinter.Header("Статистика");
            ConsolePrinter.PrintStatistics(_service.GetStatistics());
        }

        private void ShowDetails()
        {
            ConsolePrinter.Header("Деталі та історія");
            Parcel parcel = _service.GetByTrackingNumber(InputReader.ReadLine("Трекінг-номер: "));
            ConsolePrinter.PrintDetails(parcel);
            Console.WriteLine("Історія статусів:");
            ConsolePrinter.PrintHistory(parcel);
        }
    }
}
