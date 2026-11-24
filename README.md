# Служба кур'єрської доставки (Курсова робота з ООП, варіант 25)

Консольне застосування на C# (.NET 8): облік посилок, розрахунок вартості доставки за вагою, об'ємом і відстанню,
відстеження статусів. Дані зберігаються у `data.json` (серіалізація `System.Text.Json`).

## Структура

| Проєкт | Тип | Призначення |
|---|---|---|
| `Courier.Core` | Class Library (DLL) | Моделі, інтерфейси, сервіси, репозиторій, компаратори, виключення, серіалізація |
| `Courier.UI` | Console App | Меню, валідація вводу, вивід; посилається на `Courier.Core` |
| `Courier.Tests` | xUnit | 45 модульних тестів бізнес-логіки |

Додатково: `docs/Poyasnyuvalna_zapyska_variant25.docx` (пояснювальна записка), `docs/screenshots/` (скріншоти),
`docs/ZAHYST_shpargalka.md` (питання до захисту).

## Збірка та запуск

Потрібен [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```
dotnet build
dotnet test
dotnet run --project Courier.UI                 # data.json поруч із exe
dotnet run --project Courier.UI -- C:\tmp\my.json   # власний файл даних
```

## Демонстраційний сценарій (для захисту)

1. Пункт **1** — додати посилку; спробувати ввести ПІБ `123`, телефон `abc`, вагу `abc` — валідація відхилить.
2. Пункт **1** з вагою `45` → `WeightLimitExceededException`; з містом одержувача `Атлантида` → `AddressNotFoundException`.
3. Пункт **2** — розрахунок вартості Standard і Express.
4. Пункт **3** — Created → Accepted → InTransit; потім спроба → Delivered (`InvalidStatusTransitionException`).
5. Пункти **7–10** — пошук, фільтр, сортування (різні `IComparer`), статистика.
6. Пункт **0** — вихід зі збереженням; повторний запуск — дані завантажуються (`Завантажено посилок: N`).

## Тарифи (`Courier.Core/Services/PricingConfig.cs`)

`Вартість = (50 + розрахункова_вага × 12 + км × 0,6) × множник`, множник: Standard 1,0 / Express 1,5.
Розрахункова вага = max(фактична, Д×Ш×В / 5000). Ліміт ваги — 30 кг.
Відстані між містами в `AddressDirectory` — орієнтовні автомобільні.

## Відтворення скріншотів і записки

```
powershell -ExecutionPolicy Bypass -File docs\tools\make-screenshots.ps1
powershell -ExecutionPolicy Bypass -File docs\tools\build-docx.ps1
```
