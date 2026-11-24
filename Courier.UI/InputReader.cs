using System.Globalization;
using Courier.Core.Exceptions;

namespace Courier.UI
{
    /// <summary>Виняток, що сигналізує про завершення вхідного потоку (Ctrl+Z / кінець файлу).</summary>
    public class InputEndedException : Exception
    {
        public InputEndedException() : base("Вхідний потік завершено.")
        {
        }
    }

    /// <summary>Безпечне читання та валідація даних з консолі. Повторює запит до коректного вводу.</summary>
    public static class InputReader
    {
        public static string ReadLine(string prompt)
        {
            Console.Write(prompt);
            string? line = Console.ReadLine();
            if (line == null) throw new InputEndedException();

            // При вводі з файлу/каналу термінал не показує набране — дублюємо його для читабельності журналу.
            if (Console.IsInputRedirected) Console.WriteLine(line);
            return line.Trim();
        }

        public static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                string input = ReadLine(prompt);
                if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
                    && result >= min && result <= max)
                    return result;
                ConsolePrinter.Error($"Помилка! Введіть ціле число від {min} до {max}.");
            }
        }

        /// <summary>Читає дійсне число; приймає і крапку, і кому як роздільник.</summary>
        public static double ReadDouble(string prompt, double min, double max)
        {
            while (true)
            {
                string input = ReadLine(prompt).Replace(',', '.');
                if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double result)
                    && !double.IsInfinity(result) && result >= min && result <= max)
                    return result;
                ConsolePrinter.Error($"Помилка! Введіть число від {min} до {max}.");
            }
        }

        public static decimal ReadDecimal(string prompt, decimal min, decimal max)
        {
            while (true)
            {
                string input = ReadLine(prompt).Replace(',', '.');
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result)
                    && result >= min && result <= max)
                    return result;
                ConsolePrinter.Error($"Помилка! Введіть число від {min} до {max}.");
            }
        }

        /// <summary>
        /// Читає рядок і передає його в apply (зазвичай — присвоєння властивості моделі).
        /// Якщо модель відхиляє значення — показує причину й просить ввести знову.
        /// </summary>
        public static void ReadInto(string prompt, Action<string> apply, bool allowEmpty = false)
        {
            while (true)
            {
                string input = ReadLine(prompt);
                if (allowEmpty && input.Length == 0)
                {
                    apply(string.Empty);
                    return;
                }
                try
                {
                    apply(input);
                    return;
                }
                catch (InvalidParcelDataException ex)
                {
                    ConsolePrinter.Error(ex.Message);
                }
            }
        }

        public static bool ReadYesNo(string prompt)
        {
            while (true)
            {
                string input = ReadLine(prompt + " (т/н): ").ToLowerInvariant();
                if (input is "т" or "так" or "y" or "yes") return true;
                if (input is "н" or "ні" or "n" or "no") return false;
                ConsolePrinter.Error("Помилка! Введіть 'т' або 'н'.");
            }
        }

        /// <summary>Вибір значення enum за номером зі списку.</summary>
        public static T ReadEnum<T>(string title, IReadOnlyList<T> options) where T : struct, Enum
        {
            Console.WriteLine(title);
            for (int i = 0; i < options.Count; i++)
                Console.WriteLine($"  {i + 1}. {options[i]}");
            int choice = ReadInt("Ваш вибір: ", 1, options.Count);
            return options[choice - 1];
        }
    }
}
