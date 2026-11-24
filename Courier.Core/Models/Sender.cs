using System.Text.RegularExpressions;
using Courier.Core.Exceptions;

namespace Courier.Core.Models
{
    /// <summary>Відправник посилки.</summary>
    public class Sender : Person
    {
        private string _email = string.Empty;

        /// <summary>Email (необов'язковий, але якщо вказаний — має бути коректним).</summary>
        public string Email
        {
            get => _email;
            set
            {
                if (!string.IsNullOrWhiteSpace(value) &&
                    !Regex.IsMatch(value.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    throw new InvalidParcelDataException("Некоректний формат email.");
                _email = value?.Trim() ?? string.Empty;
            }
        }

        public override string Role => "Відправник";
    }
}
