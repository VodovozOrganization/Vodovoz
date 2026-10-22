# Mindbox

Интеграция с Mindbox:

- `Vodovoz.Mindbox` — библиотека с DTO, маппингом, HTTP-клиентом и сервисами;
- `MindboxCounterpartySender` — воркер, который разбирает очередь и отправляет контрагентов в Mindbox с ретраями;
- `Mindbox.CounterpartiesExport` — консольная утилита первичной выгрузки контрагентов с 01.01.2025 через bulk-import.

## Схема работы

1. При создании контрагента в Desktop (`CounterpartyDlg.Save`) в той же транзакции добавляется запись в `mindbox_counterparty_outbox`.
2. Воркер `MindboxCounterpartySender` выбирает сообщения со статусом `Pending`, формирует JSON по актуальным данным контрагента и вызывает операцию создания в Mindbox.
3. При ошибке сообщение получает следующую попытку с экспоненциальной задержкой (60 сек, 120 сек, 240 сек...). После `Mindbox.MaxSendAttempts` попыток статус становится `Failed`.
4. `transactionId` хранится в сообщении и повторно используется при ретраях — Mindbox не создаст дубль.

## Сборка

Проекты интеграции собраны в отдельное решение `Mindbox.sln`:

```powershell
dotnet build Applications/Backend/Workers/Docker/Mindbox/Mindbox.sln
```

Основной `Vodovoz.sln` не менялся: Desktop использует только сущность очереди и сервис постановки из `VodovozBusiness`, поэтому новые проекты не нужны в его графе зависимостей. Воркер и утилиту удобно собирать/публиковать отдельно.


## Настройки в `base_parameters`

| Настройка | Обязательна | Значение по умолчанию | Описание |
|---|---|---|---|
| `Mindbox.IsEnabled` | нет | `false` | Включает постановку в очередь и работу воркера |
| `Mindbox.BaseUrl` | нет | `https://api.mindbox.ru` | Базовый адрес API |
| `Mindbox.SecretKey` | да | — | Секретный ключ проекта |
| `Mindbox.EndpointId` | да | — | Идентификатор точки доступа |
| `Mindbox.CreateCounterpartyOperation` | да | — | Системное имя операции создания контрагента |
| `Mindbox.CreateCounterpartyOperationType` | нет | `async` | Тип операции: `sync` или `async` |
| `Mindbox.ImportOperation` | нет | `DirectCrm.Customers.Import` | Операция массового импорта |
| `Mindbox.ImportSegment` | нет | пусто | Сегмент для массового импорта |
| `Mindbox.ImportSourceActionTemplate` | нет | пусто | Действие регистрации клиента |
| `Mindbox.ImportBatchSize` | нет | `20000` | Строк CSV в пакете |
| `Mindbox.SendBatchSize` | нет | `50` | Сообщений очереди за цикл воркера |
| `Mindbox.MaxSendAttempts` | нет | `10` | Максимум попыток отправки |
| `Mindbox.RetryDelaySeconds` | нет | `60` | Базовая задержка ретрая |

Скрипты:

- `sql/001_mindbox_counterparty_outbox.sql` — таблица очереди;
- `sql/002_mindbox_settings.sql` — настройки в `base_parameters`.

> Для повторных отправок без дублей операция создания контрагента должна быть асинхронной (`async`) либо поддерживать `transactionId`. Если операция синхронная и не поддерживает ключ идемпотентности, ретраи могут привести к повторному созданию — согласуйте тип операции с менеджером Mindbox.

## Первичная выгрузка

```powershell
dotnet run --project Applications/Backend/Workers/Docker/Mindbox/Mindbox.CounterpartiesExport -- \
  --from 2025-01-01
```

Полезные режимы:

```powershell
# Только сформировать CSV-файлы, без отправки
dotnet run --project ... -- --from 2025-01-01 --dry-run

# Сохранить один общий CSV в файл
dotnet run --project ... -- --from 2025-01-01 --output C:\temp\mindbox-counterparties.csv
```

Перед запуском заполните `DatabaseConnectionSettings` в `appsettings.json` (или переменные окружения) и настройки Mindbox в `base_parameters`.

Mindbox принимает не более 60 импортов в час. При `ImportBatchSize = 20000` это примерно 1,2 млн строк в час; для больших объёмов увеличьте размер пакета или запускайте выгрузку частями.

### Колонки CSV

`Websiteid;FirstName;LastName;MiddleName;BirthDate;MobilePhone;Email;SourceDateTimeUtc;SourcePointOfContact;IsSubscribed;ERPcounterpartyId;CounterpartyForm;CounterpartyType;IsNet;OwnershipForm;CounterpartyName;CounterpartyFullName;PaymentForm;INN;KPP`

Особенности:

- `BirthDate` в ERP не хранится, колонка остается пустой (нужна для совместимости с шаблоном импорта);
- `IsSubscribed = true`, если у контрагента заполнен e-mail, иначе `false`;
- `Websiteid` и `ERPcounterpartyId` — код контрагента в Доставке воды;
- `transactionId` пакета детерминирован (дата + номер пакета + максимальный Id контрагента), поэтому повторный запуск не создает дубли;
- в JSON создания `DeliveryPointFields.source` передается как `0`: отдельный источник точки доставки в ERP не хранится.

## Ограничения

- Интеграция ставит в очередь только создание контрагента через карточку Desktop. Создание через мобильное приложение/API и импорт из 1С нужно подключать отдельно вызовом `IMindboxCounterpartyOutboxService.Enqueue`.
- Для работы воркера и утилиты нужны таблица `mindbox_counterparty_outbox` и настройки из `sql/`.
- Если `Mindbox.IsEnabled` отсутствует или равно `false`, запись в очередь не выполняется.
