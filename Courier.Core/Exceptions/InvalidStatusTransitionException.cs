using Courier.Core.Enums;

namespace Courier.Core.Exceptions
{
    /// <summary>Недопустимий перехід між статусами посилки.</summary>
    public class InvalidStatusTransitionException : Exception
    {
        public ParcelStatus From { get; }
        public ParcelStatus To { get; }

        public InvalidStatusTransitionException(ParcelStatus from, ParcelStatus to)
            : base($"Недопустимий перехід статусу: {from} -> {to}.")
        {
            From = from;
            To = to;
        }
    }
}
