using System.Text;
using Courier.Core.Exceptions;
using Courier.Core.Models;
using Courier.Core.Repositories;
using Courier.Core.Services;
using Courier.UI;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Шлях до файлу даних: перший аргумент командного рядка або data.json поруч із exe.
string dataPath = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "data.json");

var fileService = new JsonFileService<Parcel>(dataPath);
var repository = new ParcelRepository();
var service = new ParcelService(repository, new AddressDirectory());

// Завантаження даних на старті
try
{
    repository.AddRange(fileService.Load());
    ConsolePrinter.Info($"Завантажено посилок: {repository.Count} (файл: {dataPath})");
}
catch (DataFileCorruptedException ex)
{
    ConsolePrinter.Error(ex.Message);
    ConsolePrinter.Info("Починаємо з порожньої бази.");
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    ConsolePrinter.Error($"Не вдалося прочитати файл даних: {ex.Message}");
}

try
{
    new MenuController(service).Run();
}
catch (InputEndedException)
{
    ConsolePrinter.Info("Вхідний потік завершено — завершуємо роботу.");
}

// Збереження перед виходом
try
{
    fileService.Save(repository.GetAll());
    ConsolePrinter.Success($"Дані збережено ({repository.Count} посилок). До побачення!");
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    ConsolePrinter.Error($"Не вдалося зберегти дані: {ex.Message}");
}
