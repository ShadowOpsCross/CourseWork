namespace Courier.Core.Exceptions
{
    /// <summary>Файл даних пошкоджений і не може бути прочитаний.</summary>
    public class DataFileCorruptedException : Exception
    {
        public DataFileCorruptedException(string filePath, Exception inner)
            : base($"Файл даних \"{filePath}\" пошкоджений: {inner.Message}", inner)
        {
        }
    }
}
