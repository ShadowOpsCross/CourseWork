using Courier.Core.Models;

namespace Courier.Tests
{
    internal static class TestData
    {
        public static Sender CreateSender(string city = "Київ") => new()
        {
            FullName = "Іван Петренко",
            Phone = "+380501234567",
            City = city,
            Email = "ivan@example.com"
        };

        public static Recipient CreateRecipient(string city = "Львів") => new()
        {
            FullName = "Олена Коваль",
            Phone = "0671234567",
            City = city,
            Street = "Шевченка",
            Building = "10"
        };
    }
}
