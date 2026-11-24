namespace Courier.Core.Exceptions
{
    /// <summary>Некоректні дані посилки або учасників доставки.</summary>
    public class InvalidParcelDataException : Exception
    {
        public InvalidParcelDataException(string message) : base(message)
        {
        }
    }
}
