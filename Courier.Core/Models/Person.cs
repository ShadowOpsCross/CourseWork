using System.Text.RegularExpressions;
using Courier.Core.Exceptions;

namespace Courier.Core.Models
{
    /// <summary>Абстрактний базовий клас учасника доставки (відправник / одержувач).</summary>
    public abstract class Person
    {
        private static readonly Regex NameRegex =
            new(@"^\p{L}[\p{L}\s'’\-\.]{1,59}$", RegexOptions.Compiled);

        private string _fullName = string.Empty;
        private string _phone = string.Empty;
        private string _city = string.Empty;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName
        {
            get => _fullName;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || !NameRegex.IsMatch(value.Trim()))
                    throw new InvalidParcelDataException(
                        "ПІБ має містити 2-60 символів (літери, пробіл, дефіс, апостроф).");
                _fullName = value.Trim();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                string digits = Regex.Replace(value ?? string.Empty, @"[\s\-\(\)]", "");
                if (!Regex.IsMatch(digits, @"^\+?\d{10,13}$"))
                    throw new InvalidParcelDataException(
                        "Телефон має містити 10-13 цифр (допускається початковий '+').");
                _phone = digits;
            }
        }

        public string City
        {
            get => _city;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidParcelDataException("Місто не може бути порожнім.");
                _city = value.Trim();
            }
        }

        /// <summary>Роль учасника для відображення.</summary>
        public abstract string Role { get; }

        public override string ToString() => $"{FullName} ({Phone}), {City}";
    }
}
