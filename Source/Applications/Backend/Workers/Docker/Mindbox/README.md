# Mindbox

Интеграция отправляет контрагентов в Mindbox через MassTransit outbox шины `online_orders`.

## Состав

- `Mindbox.Library` — DTO, нормализация, маппинг, HTTP-клиент, consumer, public retry.
- `Mindbox.CounterpartySender` — воркер: ставит новых и старых контрагентов в outbox и обрабатывает отправку.
- `Mindbox.CounterpartiesAdmin` — отчёт по ошибкам и повторная отправка вручную.

`MindboxCounterpartySendConsumer` — не отдельный сервис и не отдельный контейнер. Он регистрируется внутри
`Mindbox.CounterpartySender` через `busConf.AddConsumer<...>()`, поэтому один воркер одновременно:
1) публикует сообщения в outbox (hosted service `CounterpartySenderWorker`);
2) принимает их из очереди `mindbox-counterparty-send` и отправляет в Mindbox (MassTransit consumer).

`Mindbox.IsEnabled` управляет только постановкой в очередь. Если сообщения уже лежат в outbox/очереди,
consumer обработает их даже при выключенной настройке.

## API

`POST https://api.s.mindbox.ru/v3/operations/sync?endpointId=vodovoz-spb.Erp&operation=RegisterCustomer&deviceUUID=...&transactionId=...`

- `deviceUUID` передаётся только для контрагентов, созданных из ИПЗ (мобильное приложение, сайт, ИИ-бот). Берётся из `ExternalCounterparty.ExternalCounterpartyId`.
- Для контрагентов, созданных менеджером вручную, `deviceUUID` не передаётся.
- В теле используется `customer.ids.externalid`.
- `Authorization: SecretKey <key>`.

## Статусы

Состояние хранится в `mindbox_counterparty_state` (1 запись на контрагента):

- `Unknown` — не отправлялся;
- `Queued` — в очереди;
- `Sent` — отправлен;
- `DataError` — ошибка данных;
- `Error` — ошибка.

Там же: `transaction_id`, `attempts`, `last_attempt_at`, `last_error`, `last_http_status_code`, `last_mindbox_status`, `last_error_id`, `sent_at`.

## Очередь и повторы

- Воркер ищет контрагентов без записи состояния или со статусом `Unknown` — так в queue попадают все старые и новые контрагенты.
- Публикация сообщения и смена статуса на `Queued` выполняются в одной транзакции через MassTransit outbox.
- Consumer: 1 м, 5 м, 15 м, 1 ч, далее раз в час в течение 24 часов.
- 500/502/503/504, таймаут, ошибка соединения — повтор.
- 400, 401/403, 404, 429, `ValidationError` — без повтора.
- 401/403/404/429 — пауза всей очереди, алерт, проверка через 5 минут.
- После 24 часов — `Error` и алерт.

## SQL

Для чистой установки:

1. `sql/002_mindbox_settings.sql`
2. `sql/006_mindbox_point_of_contact.sql`
3. `sql/010_mindbox_counterparty_state.sql`
4. `sql/013_mindbox_bonus_settings.sql`
5. `sql/014_mindbox_authorized_cart_settings.sql`

Если ранее уже применялись `007`, `008`, `009` (статус и transactionId лежали в `counterparty`):

1. `sql/002_mindbox_settings.sql`
2. `sql/006_mindbox_point_of_contact.sql`
3. `sql/010_mindbox_counterparty_state.sql`
4. `sql/011_mindbox_migrate_status_to_state.sql`
5. `sql/013_mindbox_bonus_settings.sql`
6. `sql/014_mindbox_authorized_cart_settings.sql`

`007`–`009` — устаревшие скрипты промежуточной версии, на чистой базе их применять не нужно.

`013_mindbox_bonus_settings.sql` идемпотентен и добавляет только новые настройки расчёта бонусов. Его можно применять
на любой уже настроенной базе после `002`.

Если таблица `mindbox_counterparty_state` уже создавалась предыдущей версией `010` (signed int и `transaction_id NOT NULL`), после `010` выполните `sql/012_mindbox_state_types.sql` — он приводит типы к `unsigned int(10)` и делает `transaction_id` nullable.

## Настройки `base_parameters`

| Настройка | Значение |
|---|---|
| `Mindbox.IsEnabled` | `false` / `true` |
| `Mindbox.BaseUrl` | `https://api.s.mindbox.ru` |
| `Mindbox.EndpointId` | `vodovoz-spb.Erp` |
| `Mindbox.CreateCustomerOperation` | `RegisterCustomer` |
| `Mindbox.CreateCustomerOperationType` | `sync` |
| `Mindbox.SecretKey` | тестовый/боевой ключ |
| `Mindbox.PointOfContactErp` | `1` |
| `Mindbox.TimeoutSeconds` | `10` |
| `Mindbox.RetryScheduleMinutes` | `1,5,15,60` |
| `Mindbox.RetryIntervalMinutes` | `60` |
| `Mindbox.RetryMaxHours` | `24` |
| `Mindbox.PauseMinutes` | `5` |
| `Mindbox.TransientErrorsBeforePause` | `3` |
| `Mindbox.UnavailableAlertMinutes` | `15` |
| `Mindbox.EnqueueBatchSize` | `1000` |
| `Mindbox.SendBatchSize` | `50` |
| `Mindbox.BonusCalculationEnabled` | `false` / `true` — расчёт бонусов в ценах МП |
| `Mindbox.CalculateProductListOperation` | `CalculateProductList` |
| `Mindbox.BonusItemIdName` | `itemErpId` |
| `Mindbox.BonusBalanceSystemName` | системное имя балльного счёта из Mindbox |
| `Mindbox.BonusBatchSize` | `100` |
| `Mindbox.BonusMaxParallelBatches` | `5` |
| `Mindbox.BonusBatchTimeoutSeconds` | `2` |
| `Mindbox.BonusTotalTimeoutMilliseconds` | `2500` |
| `Mindbox.BonusErrorsBeforePause` | `5` |
| `Mindbox.BonusPauseMinutes` | `1` |
| `Mindbox.BonusCacheSeconds` | `0` — кеш выключен |
| `Mindbox.CalculateAuthorizedCartOperation` | `CalculateAuthorizedCart` — операция расчёта бонусов в заказе |
| `Mindbox.AuthorizedCartTimeoutSeconds` | `3` — таймаут CalculateAuthorizedCart |
| `Mindbox.BonusProgramMode` | `Disabled` / `Clients` / `All` — фича-флаг бонусной программы |
| `Mindbox.BonusProgramClientIds` | список id клиентов ERP через запятую для режима `Clients` |

Тестовый и боевой контуры — отдельные базы ERP со своими `EndpointId`, операцией и ключом. Тестовый ключ Mindbox действует 12 часов.

## Расчёт бонусов в ценах МП

Метод `GET /api/v3/SaleItem/GetSaleItemsPricesAndStocks` при `source = 54` (мобильное приложение) и включённом
`Mindbox.BonusCalculationEnabled` дополняет каждую позицию полем `bonusAmount` — сколько бонусов начисляется за 1 шт.
по базовой цене (цена из `prices[]` с минимальным `minCount`).

V2-метод `/api/v2/SaleItem/GetSaleItemsPricesAndStocks` не изменяется и поле `bonusAmount` не содержит.
Для V3 продублирован слой V2 (DTO, репозиторий, конвертер, фабрика, сервис), а в `SaleItemService` добавлен расчёт бонусов.

- Вызывается операция `CalculateProductList` (sync), ключ товара собирается тем же `MindboxItemIdBuilder`, что и в каталоге:
  `product_{erpId}`, `promoset_{erpId}`, `rentpackage_{erpId}`.
- Позиции отправляются пачками по `Mindbox.BonusBatchSize`, одновременно не более `Mindbox.BonusMaxParallelBatches`.
- Общий лимит ожидания — `Mindbox.BonusTotalTimeoutMilliseconds`. Неответившие и ошибочные пачки получают `null`,
  остальные позиции рассчитываются.
- Ошибки Mindbox не ломают метод: ответ 200 с ценами и остатками возвращается всегда.
- После `Mindbox.BonusErrorsBeforePause` подряд идущих 5xx/429/таймаутов вызовы приостанавливаются на
  `Mindbox.BonusPauseMinutes`; после паузы выполняется один пробный вызов.
- При `Mindbox.BonusCacheSeconds > 0` последний успешный ответ кешируется по связке «ключ товара + базовая цена».
- Тела запросов/ответов и секретный ключ в лог не пишутся; логируются количество позиций и пачек, успешные пачки,
  время ожидания и параметры ошибок.

## Что развернуть

1. **БД**: применить SQL из раздела выше.
2. **RabbitMQ**: vhost `online_orders` (для теста — `online_orders_test`), пользователь с правами на очередь `mindbox-counterparty-send`.
3. **Таблицы MassTransit outbox** `online_orders_outbox_*` — общая инфраструктура проекта, должна быть развернута для шины `online_orders`.
4. **Воркер** `Mindbox.CounterpartySender`:
   - .NET 5 runtime или Docker-образ;
   - `DatabaseConnectionSettings`;
   - `MessageTransport` (Host, Port, VirtualHost, Username, Password, UseSSL);
   - настройки Zabbix;
   - запуск как службы/контейнера.
5. **Админ-утилита** `Mindbox.CounterpartiesAdmin` — для отчёта и повторной отправки (можно запускать по требованию, не держать постоянно).
6. **Mindbox**: операция `RegisterCustomer` (sync), endpoint `vodovoz-spb.Erp`, ключ, при необходимости — IP-whitelist.
7. Включить `Mindbox.IsEnabled = true` после заполнения ключа и проверки тестового контура.

## Отчёт и повторная отправка

```powershell
$admin = "Applications/Backend/Workers/Docker/Mindbox/Mindbox.CounterpartiesAdmin"

dotnet run --project $admin -- report
dotnet run --project $admin -- report --status DataError,Error --output C:\temp\mindbox-errors.csv
dotnet run --project $admin -- resend --id 123
dotnet run --project $admin -- resend --ids 1,2,3
dotnet run --project $admin -- resend --all-errors
```

При повторной отправке после `DataError` создаётся новый `transactionId`; после `Error` сохраняется прежний — на случай, если Mindbox уже принял запрос.

## Нормализация

- телефон приводится к `+7XXXXXXXXXX`, некорректный не отправляется;
- e-mail обрезается и приводится к нижнему регистру, некорректный не отправляется;
- ФИО: обрезка, схлопывание пробелов, регистр «Иван Петров»;
- название юрлица: обрезка и схлопывание пробелов без изменения регистра.

## Замечание по безопасности

`appsettings.Development.json` содержит тестовые креды. Не использовать этот файл в production; боевые значения передавать через переменные окружения/секреты.

## Расчёт бонусов при оформлении заказа

`POST /api/v8/ApplyBonusToOrder` принимает корзину из последнего ответа Apply-методов и сумму списания, вызывает
операцию Mindbox `CalculateAuthorizedCart` и возвращает баланс, доступное списание, начисление и позиции заказа
с полем `bonusesWrittenOff`. Метод ничего не сохраняет и не пересчитывает цены и скидки ERP.

- Mindbox вызывается только при `source = 54`, включённом фича-флаге `Mindbox.BonusProgramMode` и статусе
  контрагента `Sent` в `mindbox_counterparty_state`.
- Если контрагент ещё не отправлялся (`Unknown` или нет записи), он ставится в очередь отправки, а метод возвращает
  `CustomerNotRegistered` без вызова Mindbox.
- Таймаут запроса — `Mindbox.AuthorizedCartTimeoutSeconds` (по умолчанию 3 секунды). После
  `Mindbox.BonusErrorsBeforePause` ошибок 5xx/429/таймаутов подряд Mindbox не вызывается
  `Mindbox.BonusPauseMinutes` минут.
- Позиции платной доставки и доставки за час не отправляются в `lines`, их сумма уходит в `deliveryCost`,
  списание бонусов на них равно 0.
- Ключ позиции собирает `MindboxItemIdBuilder`: `product_{erpId}`, `promoset_{erpId}`, `rentpackage_{erpId}`.
- Сбой Mindbox не является ошибкой метода: возвращается 200 с `isAvailable = false` и
  `unavailableReason = ServiceUnavailable`.

Для включения программы задайте `Mindbox.BonusProgramMode` (`All` или `Clients` со списком
`Mindbox.BonusProgramClientIds`), системное имя балльного счёта в `Mindbox.BonusBalanceSystemName`, а также
актуальные `Mindbox.EndpointId` и `Mindbox.SecretKey` для теста и прода.
