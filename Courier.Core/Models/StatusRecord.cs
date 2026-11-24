using Courier.Core.Enums;

namespace Courier.Core.Models
{
    /// <summary>Запис в історії статусів посилки.</summary>
    public class StatusRecord
    {
        public ParcelStatus Status { get; set; }
        public DateTime Timestamp { get; set; }

        public StatusRecord()
        {
        }

        public StatusRecord(ParcelStatus status, DateTime timestamp)
        {
            Status = status;
            Timestamp = timestamp;
        }
    }
}
