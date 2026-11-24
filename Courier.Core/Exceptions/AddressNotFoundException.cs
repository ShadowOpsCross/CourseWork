namespace Courier.Core.Exceptions
{
    /// <summary>Адресу (населений пункт) не знайдено в довіднику.</summary>
    public class AddressNotFoundException : Exception
    {
        public string Address { get; }

        public AddressNotFoundException(string address)
            : base($"Адресу не знайдено в довіднику: \"{address}\".")
        {
            Address = address;
        }
    }
}
