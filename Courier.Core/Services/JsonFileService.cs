using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Courier.Core.Exceptions;

namespace Courier.Core.Services
{
    /// <summary>Збереження та завантаження списку об'єктів через серіалізацію JSON.</summary>
    public class JsonFileService<T>
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly string _filePath;

        public JsonFileService(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Шлях до файлу не може бути порожнім.", nameof(filePath));
            _filePath = filePath;
        }

        public string FilePath => _filePath;

        /// <summary>Записує дані через тимчасовий файл, щоб не втратити старі дані при збої.</summary>
        public void Save(List<T> data)
        {
            string json = JsonSerializer.Serialize(data, Options);
            string tempPath = _filePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _filePath, overwrite: true);
        }

        /// <summary>Читає дані. Якщо файлу немає — повертає порожній список.</summary>
        /// <exception cref="DataFileCorruptedException">Файл містить некоректний JSON.</exception>
        public List<T> Load()
        {
            if (!File.Exists(_filePath)) return new List<T>();

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
            }
            catch (Exception ex) when (ex is JsonException || ex is Exceptions.InvalidParcelDataException
                                       || ex is WeightLimitExceededException)
            {
                throw new DataFileCorruptedException(_filePath, ex);
            }
        }
    }
}
