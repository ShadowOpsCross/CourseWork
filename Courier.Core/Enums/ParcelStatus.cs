namespace Courier.Core.Enums
{
    /// <summary>Статус посилки в процесі доставки.</summary>
    public enum ParcelStatus
    {
        Created,
        Accepted,
        InTransit,
        OutForDelivery,
        Delivered,
        Returned
    }
}
