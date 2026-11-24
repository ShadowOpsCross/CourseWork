using System.Text.Json.Serialization;
using Courier.Core.Exceptions;

namespace Courier.Core.Models
{
    /// <summary>Одержувач посилки (має повну адресу доставки).</summary>
    public class Recipient : Person
    {
        private string _street = string.Empty;
        private string _building = string.Empty;

        public string Street
        {
            get => _street;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidParcelDataException("Вулиця не може бути порожньою.");
                _street = value.Trim();
            }
        }

        public string Building
        {
            get => _building;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidParcelDataException("Номер будинку не може бути порожнім.");
                _building = value.Trim();
            }
        }

        [JsonIgnore]
        public string FullAddress => $"{City}, {Street}, {Building}";

        public override string Role => "Одержувач";
    }
}
