# Публикация сообщений через outbox MassTransit

Все сообщения в RabbitMQ публикуются через outbox: сообщение записывается в таблицы outbox MassTransit
в той же транзакции UoW, что и данные, а в брокер его отправляет служба отправки MassTransit после коммита.
Если транзакция откатилась, сообщение не уйдёт. Если брокер недоступен, сообщение не потеряется: строка остаётся
в таблице, пока служба отправки MassTransit её не отправит.

Механизм один для бэкенда и десктопа: сообщение пишет в таблицы NHibernate (`UnitOfWorkOutboxContext`, наше),
отправляет служба отправки MassTransit из таблиц через EF Core (`OutboxDbContext`, штатное).
Inbox MassTransit не используется: потребители должны переживать повторную доставку сообщения.

## Правило

**Публикуй в UoW, в которой меняешь данные, до `Commit`. По возможности — через `IPublishEndpoint` из того же DI-scope,
что и UoW.**

```csharp
// UoW и IPublishEndpoint из одного DI-scope — потребитель, контроллер, проход воркера (бэкенд):
await _publishEndpoint.Publish(message, cancellationToken);   // IPublishEndpoint из того же DI-scope
await _unitOfWork.CommitAsync(cancellationToken);             // сохраняет и данные, и сообщение

// UoW, которого нет в DI-scope: десктоп, UoW параметром метода, вторая шина:
await _outbox.GetPublishEndpoint(uow).Publish(message, cancellationToken);   // IUnitOfWorkOutbox<IBus>
await uow.CommitAsync(cancellationToken);
```

`Commit` обязателен и тогда, когда данные не менялись, в том числе в потребителе: без него сообщение не сохранится.

Данные не меняются, а сообщение отправить нужно — своя короткая транзакция:

```csharp
await _outbox.PublishInNewTransactionAsync(_unitOfWorkFactory, message, cancellationToken);
```

## Как сообщение попадает в RabbitMQ

Публикация ничего не отправляет в брокер: она только добавляет строки в таблицы outbox. Дальше работает служба отправки
MassTransit (в MassTransit она называется службой доставки outbox, `BusOutboxDeliveryService`; не путать с функцией
MassTransit Courier). Сервис, в котором она отправляет сообщения нужной шины, ниже называется сервисом-отправителем шины.

1. **Запись.** При `Publish` MassTransit сам сериализует сообщение (тело, заголовки, адрес назначения в RabbitMQ)
   штатными классами outbox MassTransit, а наш `UnitOfWorkOutboxContext` сохраняет результат через сессию NHibernate
   этого UoW: строка в `X_outbox_state` на транзакцию и строка в `X_outbox_message` на каждое сообщение
   (таблицы и их колонки — штатные таблицы bus outbox MassTransit). `X` — шина (vhost), например `pacs`.
   Префикс шины уйдёт после перехода на один vhost: таблицы останутся одни.
2. **Коммит.** `uow.Commit()` сохраняет данные и строки outbox одной транзакцией. До коммита строк никто не видит,
   после отката их нет.
3. **Отправка.** В каждом сервисе с `AddVodovozOutbox(OutboxBusNames.X)` работает штатная служба отправки MassTransit.
   Всё дальнейшее делает сам outbox MassTransit: раз в несколько секунд служба читает через EF Core закоммиченные
   строки таблиц своей шины, блокирует их (несколько экземпляров одну строку не возьмут), отправляет сообщения одной
   транзакции по порядку своей шиной и удаляет строки.
4. **Получение.** Сообщение уходит в exchange RabbitMQ (имя exchange и маршрутизацию задаёт топология MassTransit)
   и попадает в очереди тех, кто на него подписан. Получатель — обычный потребитель MassTransit, чаще всего в другом сервисе.

```
Сервис A:  данные + строка в pacs_outbox_message ── один Commit
                                   │
Сервис с основной шиной pacs (сервис-отправитель): служба отправки MassTransit читает строку ──► RabbitMQ, vhost pacs ──► очередь ──► потребитель
```

Сервису-отправителю не нужен потребитель этого сообщения и даже его тип: строка уже содержит готовое сообщение с адресом,
служба отправки MassTransit отправляет его как есть. Важно только, чтобы сервис-отправитель
был подключён к vhost из адреса — поэтому каждый сервис отправляет сообщения только из таблиц своей шины
(после перехода на один vhost это ограничение уйдёт).

Задержка — до ~10 с. В штатном outbox MassTransit при записи через EF Core служба отправки получает сигнал сразу,
а при нашей записи через NHibernate сигнала нет, и она находит строки при очередном опросе (интервал опроса —
настройка outbox MassTransit).

## Откуда брать UoW

| Где код | Как получить UoW и публикацию |
|---|---|
| Контроллер, потребитель MassTransit | `IUnitOfWork` и `IPublishEndpoint` через конструктор. DI-scope запроса или сообщения (его на каждое сообщение создаёт MassTransit) и есть единица работы |
| Фоновый воркер (`BackgroundService`, синглтон) | на каждый проход (или элемент, которому нужна своя транзакция) — свой DI-scope, из него `IUnitOfWork` и `IPublishEndpoint`, публикация через `IPublishEndpoint` |
| Общие библиотеки, которым UoW передают параметром (в том числе с десктопа) | публикация через `_outbox.GetPublishEndpoint(uow)` в переданный UoW |
| Десктоп | `IUnitOfWork` в DI нет, у диалогов свои UoW: только `_outbox.GetPublishEndpoint(uow)`. `IPublishEndpoint` из DI на десктопе бросает исключение с подсказкой |
| Публикация во вторую шину приложения (`IEmailBus` и т. п., другой vhost) | `IUnitOfWorkOutbox<IEmailBus>.GetPublishEndpoint(uow)`, см. «Вторая шина» ниже (уйдёт после перехода на один vhost) |

**Даже в воркере-синглтоне стремимся к `IPublishEndpoint` из DI**: создаём DI-scope на проход и берём из него и UoW,
и `IPublishEndpoint` — тогда они автоматически относятся к одной транзакции, как в потребителе:

```csharp
// воркер (синглтон): IServiceScopeFactory через конструктор
using var scope = _serviceScopeFactory.CreateScope();
var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
...
await publishEndpoint.Publish(message, cancellationToken);
await uow.CommitAsync(cancellationToken);
```

Так сделано, например, в `EmailClosingDeliveriesWorker`, `ReminderToAcceptUpdEmailWorker`, `OrderContactProblemUpdateService`.
Часть воркеров пока создаёт UoW из фабрики и публикует через `IUnitOfWorkOutbox` (`EdoDocumentsPreparerWorker`,
`EmailPrepareWorker`, `FastPaymentEventsProcessor`, `TaxcomEdoDocumentFlowUpdater`, `EmailDebtNotificationService`) —
они помечены TODO, при доработке их переводят на DI-scope на проход.

Нельзя внедрять `IUnitOfWork` и `IPublishEndpoint` в конструктор синглтона (воркера): они были бы из корневого
DI-scope — одна UoW на всё время работы процесса, которую никто не коммитит.
Если в классе публикация идёт не через `IPublishEndpoint` из DI (`IUnitOfWorkOutbox`, общий публикатор, `IBus` напрямую),
над этой зависимостью пишется комментарий с причиной.

### Вторая шина

Весь раздел уйдёт после перехода на один vhost: вторых шин не будет, публикация — через `IPublishEndpoint` основной шины.

Обычно сервис работает с одним vhost RabbitMQ — это его основная шина `IBus`. Когда приложению надо отправить сообщение
в другой vhost, у него вторая шина (`AddMassTransit<IEmailBus>`, MultiBus MassTransit). `IPublishEndpoint` из DI
относится только к основной шине (так устроен MassTransit), поэтому во вторую шину публикуют явно:

```csharp
// EdoNotificationsWorker (основная шина — notifications) отправляет письмо в vhost почты
await _emailOutbox.GetPublishEndpoint(uow).Publish(sendEmailMessage);   // IUnitOfWorkOutbox<IEmailBus>
await uow.CommitAsync();
```

- Строка пишется в таблицы второй шины (`email_outbox_*`) в транзакции того же UoW, что и данные.
- Сам EdoNotificationsWorker эти таблицы не читает: его служба отправки MassTransit обслуживает только основную шину (`notifications_*`).
- Отправлять `email_outbox_*` будет любой сервис, у которого почта — основная шина (EmailPrepareWorker,
  EmailStatusUpdateWorker и др., таблица в «Эксплуатации»). Он отправит письмо в vhost почты, где его получит EmailSendWorker.
- Если ни один такой сервис не запущен, сообщения не теряются, а копятся в таблице.

Регистрация: `services.AddExplicitUnitOfWorkOutbox<TBus>(OutboxBusNames.X)`. Она регистрирует синглтон
`IUnitOfWorkOutbox<TBus>`: шина `TBus` даёт адреса своего vhost, имя `X` выбирает таблицы. Службу отправки MassTransit
она не запускает.

| Кто пишет | Вторая шина | Таблицы | Кто отправляет |
|---|---|---|---|
| EdoNotificationsWorker | `IEmailBus` | `email_*` | сервисы почты |
| Сервисы и десктоп, публикующие уведомления (`AddNotificationBusNotHosted`) | `INotificationsBus` | `notifications_*` | EdoNotificationsWorker |

`INotificationsBus` зарегистрирована «без запуска»: MassTransit её не запускает и к брокеру она не подключается,
она нужна только чтобы MassTransit сформировал строку outbox с правильным адресом.

## Готовые публикаторы

Для частых событий есть сервисы, они тоже принимают UoW и вызываются до `Commit`:

| Что | Как |
|---|---|
| Создана заявка ЭДО | `IEdoRequestCreatedEventPublisher.Publish(uow, requestId, "что создало заявку")` |
| Запустить задачу ЭДО | `IOrderEdoTaskCreatedEventPublisher.Publish(uow, edoTask)` |
| Уведомление (клиенту, по ЭДО) | `INotificationPublisher<TEvent>.TryPublishAsync(uow, domainEvent)` |

## Частые ошибки

| Ошибка | Что будет | Защита |
|---|---|---|
| `IBus.Publish` / `IBus.Send` | сообщение уходит напрямую, мимо outbox (так устроен MassTransit: `IBus` outbox не использует) | нет — только ревью |
| `context.Publish` в потребителе (`ConsumeContext` MassTransit) | тоже напрямую, без транзакции (consumer outbox MassTransit не используется). Допустимо, только если данные не меняются | нет — только ревью |
| `IPublishEndpoint` из DI на десктопе | у диалогов свои UoW, сообщению некуда записаться | ошибка при публикации с подсказкой: публиковать через `_outbox.GetPublishEndpoint(uow)` |
| `IUnitOfWork` / `IPublishEndpoint` в конструкторе синглтона (воркера) | UoW корневого scope, сообщения никогда не закоммитятся | нет — DI-scope на проход воркера |
| Нет `Commit` после публикации (в том числе в потребителе) | сообщение не сохранится | наше предупреждение в логе «UoW закрыт без Commit, опубликованные сообщения не будут отправлены» |
| `IPublishEndpoint` из DI, а UoW — из фабрики или параметром метода | сообщение уходит в UoW DI-scope, а коммитится другой UoW | то же предупреждение, если UoW scope не закоммитят. Правило: `IPublishEndpoint` из DI — только вместе с `IUnitOfWork` из того же DI-scope; UoW параметром или из фабрики — `_outbox.GetPublishEndpoint(uow)` |
| Данные закоммичены, событие о них — отдельной транзакцией | при сбое между ними событие потеряется | нет — публикуй в UoW данных до `Commit` |
| Сотни крупных сообщений (письма с вложениями) одним `Commit` | транзакция на сотни мегабайт, при сбое откатываются все | нет — `Commit` на каждый элемент (см. `ClosingDeliveriesNotificationSender`) |

Исключения, согласованные отдельно: запросы с ожиданием ответа (`IRequestClient<T>.GetResponse`, ответ нужен сразу),
телефония (Pacs, Mango), приложения, которые не пишут в БД (EmailSendWorker, CustomerNotificationsWorker,
EdoDocumentsConsumer), ручные и отладочные инструменты (EdoManualEventSender, CustomTaskDebugExecutor).

## Повторная доставка

Доставка потребителю — «как минимум один раз» (штатное поведение outbox MassTransit и RabbitMQ). Сообщение может прийти
потребителю повторно:

- служба отправки MassTransit упала между отправкой и удалением строки — после перезапуска отправит ещё раз;
- MassTransit повторяет обработку после исключения (`UseMessageRetry`);
- брокер передоставляет сообщение, если потребитель упал до подтверждения.

Inbox MassTransit этого не отсекает (он не используется, см. «Почему так»), поэтому потребитель должен переживать повтор:

- проверять состояние перед действием («задача уже в нужном статусе — ничего не делаем»);
- создавать записи под уникальным ключом (повтор упадёт на дубликате, см. регистрацию онлайн-заказов);
- помнить, что если потребитель упал после своего `Commit`, повтор может ещё раз опубликовать его сообщения.

## Эксплуатация

Если служба отправки MassTransit vhost'а стоит, сообщения не теряются, а копятся — это нужно замечать.
Сервисов-отправителей у vhost'а может быть несколько: строки между ними делит сама служба отправки MassTransit
(блокировка строк, штатное поведение outbox MassTransit), одну строку отправляет один.

Таблица ниже уйдёт после перехода на один vhost: таблицы outbox останутся одни, отправлять будет любой сервис
с `AddVodovozOutbox`.

| vhost | `OutboxBusNames` | Таблицы | Кто отправляет |
|---|---|---|---|
| pacs | `Pacs` | `pacs_*` | воркеры ЭДО (`Edo.*.Worker`), WarehouseApi, DriverAPI, ScannedTrueMarkCodesDelayedProcessingWorker, TaxcomEdoConsumer, EdoContactsUpdater, EdoDocumentFlowUpdater |
| email_host | `Email` | `email_*` | EmailPrepareWorker, EmailDebtNotificationWorker, EmailStatusUpdateWorker, CustomerAppsApi, FastPaymentEventsSender, MailganerEventsDistributorApi |
| notifications | `Notifications` | `notifications_*` | EdoNotificationsWorker (в том числе все уведомления с десктопа и других сервисов) |
| taxcom_edo_host | `Taxcom` | `taxcom_*` | EdoDocumentsPreparer |
| online_orders | `OnlineOrders` | `online_orders_*` | CustomerOrdersApi, CustomerOnlineOrdersRegistrar |

Проверки для мониторинга (для каждого префикса `pacs`, `email`, `notifications`, `taxcom`, `online_orders`;
после перехода на один vhost — для одной пары таблиц):

```sql
-- Не отправлено дольше 5 минут: стоит служба отправки MassTransit или нет связи с брокером
SELECT COUNT(*) FROM pacs_outbox_state WHERE Delivered IS NULL AND Created < UTC_TIMESTAMP() - INTERVAL 5 MINUTE;

-- Сообщения, ожидающие отправки
SELECT COUNT(*) FROM pacs_outbox_message;
```

В логах воркеров: `Deadlock found`, `Lock wait timeout` (MySQL), `R-FAULT`, `T-FAULT` (MassTransit: ошибка обработки
и отправки сообщения), наше предупреждение «UoW закрыт без Commit, опубликованные сообщения не будут отправлены».

Требование к серверу БД: `binlog_format` = `ROW` или `MIXED` (служба отправки MassTransit работает в READ COMMITTED —
настройка `IsolationLevel` штатного outbox MassTransit, задана в `AddVodovozOutbox`).

## Новый сервис бэкенда

```csharp
services.AddMassTransit(busConf =>
{
	busConf.AddVodovozOutbox(OutboxBusNames.Pacs);   // основная шина (vhost) сервиса
	busConf.ConfigureRabbitMq();
});
```

`AddVodovozOutbox` регистрирует scoped `IUnitOfWork` (UoW из фабрики, если сервис не зарегистрировал свой),
связывает с ним `IPublishEndpoint` из DI, регистрирует `IUnitOfWorkOutbox<IBus>` и запускает службу отправки MassTransit
своей шины (штатный `UseBusOutbox`). Для сервисов ЭДО — через параметр библиотеки:
`services.AddEdoMassTransit(configureBus: busConf => busConf.AddVodovozOutbox(OutboxBusNames.Pacs))`
(так же у `AddEdoDocuments`, `AddTenderEdo`, `AddCodesSaver`, `AddInformalOrderDocumentEdo`, `AddReceiptCallback`
и остальных `AddEdo*`).

Сервис отправляет сообщения одной шины: второй вызов `AddVodovozOutbox` с другой шиной — ошибка при старте.
Во вторую шину можно только писать (`AddExplicitUnitOfWorkOutbox<TBus>`), отправляют её сообщения сервисы её vhost'а.
После перехода на один vhost эта проверка и параметр шины уйдут.

Новая шина: константа в `OutboxBusNames`, классы записей и маппинги в `Domain/Buses` и `Mapping/Buses`,
ветка в `OutboxRecordFactory` и таблицы `X_outbox_state`, `X_outbox_message` в модели БД (колонки — как у штатных таблиц
bus outbox MassTransit). После перехода на один vhost новых шин не будет.

## Почему так

- **Почему не полностью штатный outbox MassTransit.** Он атомарен, только когда данные пишутся через тот же
  EF Core `DbContext`. Наши данные пишет NHibernate, а десктоп на .NET Framework 4.6.2 EF Core не поддерживает.
  Поэтому своя только запись строки через NHibernate (порт `AddSend` из MassTransit, ~190 строк), всё остальное —
  штатное MassTransit: таблицы, формат сообщения, служба отправки.
- **Почему не общая транзакция NHibernate и EF Core.** Такой мост был реализован и удалён: свой UoW поверх чужой
  транзакции, фабрика транзакций NHibernate, отложенные коммиты в потребителях — много кода ради того же результата.
- **Почему не своя отправка.** Блокировки, порядок, повторы — самая сложная часть; штатная служба отправки MassTransit
  уже проверена.
- **Почему без inbox.** Штатный inbox MassTransit требует, чтобы данные потребителя были в транзакции EF Core, — снова мост
  и запрет промежуточных коммитов. Если дубли станут реальной проблемой, дешевле свой лёгкий inbox на NHibernate
  для отдельных потребителей с одним `Commit`.
- **Почему сервисов-отправителей у vhost'а много и они же публикуют и потребляют сообщения.** В штатном outbox MassTransit у каждого
  сервиса свои таблицы outbox и служба отправки MassTransit отправляет только их; несколько отправителей бывает только
  у нескольких экземпляров одного сервиса. У нас таблицы общие на vhost, и строки отправляет любой сервис с этой
  основной шиной. Причины:
  - десктоп (.NET Framework 4.6.2, без EF Core) не может отправлять сам — его строки в любом случае отправляет
    бэкенд-сервис;
  - все сервисы и десктоп работают с одной БД: таблицы на каждый сервис дали бы десятки пар таблиц вместо пяти;
  - отдельный сервис-ретранслятор — ещё один деплой, конфиг и точка отказа; когда отправляет каждый сервис vhost'а,
    отправка продолжается, пока работает хотя бы один из них;
  - MassTransit это не мешает: строка содержит готовое сообщение с адресом, служба отправки MassTransit не проверяет,
    кто её записал, и не нуждается в потребителе или типе сообщения. Строки делятся блокировкой (штатное поведение
    outbox MassTransit), одну строку отправляет один сервис.

  Цена: служба отправки MassTransit каждого сервиса опрашивает общие таблицы (нагрузка на БД растёт с числом сервисов),
  сервисы связаны через таблицы (поэтому сервисы-отправители выкатывают первыми), а отправка с десктопа зависит от работы
  бэкенд-сервисов vhost'а. Если это станет проблемой, см. «Дальнейшие планы».

## Где что лежит

| Проект, папка | Что внутри |
|---|---|
| `TransactionalOutbox.MassTransit` | общий для десктопа и бэкенда (netstandard2.0): `IUnitOfWorkOutbox<TBus>` (`UnitOfWorkOutbox` хранит один контекст на живой UoW), запись сообщения через NHibernate (`UnitOfWorkOutboxContext`), связь `IPublishEndpoint` из DI с UoW scope на бэкенде (`UnitOfWorkScopedBusContextProvider`), запрет `IPublishEndpoint` из DI на десктопе (`ExplicitOutboxOnlyScopedBusContextProvider`), `PublishInNewTransactionAsync`, `OutboxBusNames` |
| `TransactionalOutbox.MassTransit/Domain` | записи таблиц outbox для NHibernate; `Buses/` — по классу на таблицу каждой шины (уйдёт после перехода на один vhost) |
| `TransactionalOutbox.MassTransit/Mapping` | маппинги NHibernate таблиц outbox (`Buses/` — по шине, уйдёт после перехода на один vhost); регистрируются в `AddExplicitUnitOfWorkOutbox`, поэтому нужны любому сервису без дополнительной настройки |
| `TransactionalOutbox.EntityFrameworkCore` | только бэкенд: `AddVodovozOutbox`, `OutboxDbContext` для службы отправки MassTransit (таблицы шины, заданной в `AddVodovozOutbox`) |

### Что штатное MassTransit, что наше, где EF Core

| Часть | Чьё | EF Core |
|---|---|---|
| Таблицы `X_outbox_state`, `X_outbox_message` | штатные таблицы bus outbox MassTransit | схема — штатная EF Core-модель MassTransit (`OutboxDbContext`) |
| Служба отправки (`UseBusOutbox`) | штатная MassTransit | да: читает и удаляет строки через `OutboxDbContext` |
| Запись сообщения в таблицы (`UnitOfWorkOutboxContext`, `Domain/`, `Mapping/`) | наша замена записи штатного outbox MassTransit | нет: пишет NHibernate в транзакции UoW. В штатном outbox MassTransit пишет EF Core в транзакции DbContext |
| Провайдеры конечных точек outbox, сериализация | штатные MassTransit (`OutboxPublishEndpointProvider`, `OutboxSendEndpointProvider`, `OutboxSendContext`) | нет |
| `IPublishEndpoint` из DI (`UnitOfWorkScopedBusContextProvider`) | наша замена штатного провайдера MassTransit, который регистрирует `UseBusOutbox` | нет |
| Inbox, outbox потребителя (`UseEntityFrameworkOutbox`) | штатные MassTransit, но не используются | — |

EF Core есть только в `TransactionalOutbox.EntityFrameworkCore` (бэкенд) и нужен только службе отправки MassTransit.
На десктопе EF Core нет: он только пишет строки через NHibernate.

## Дальнейшие планы

MassTransit остаётся на версии 8.0.12: версия 9 платная.

Отдельной задачей планируется переход на один vhost RabbitMQ для всех подсистем (кроме телефонии).
После него можно удалить:

- шину уведомлений «только для записи» (`INotificationsBus`, `AddNotificationBusNotHosted`): уведомления
  будут публиковаться обычным `Publish` основной шины MassTransit;
- вторые шины приложений (`IEmailBus` в EdoNotificationsWorker, `IEmailSendBus` в EmailSendWorker)
  и дубль потребителя `email.send_message.consumer`;
- наборы таблиц outbox по шинам, классы записей и маппинги в `Buses/`, `OutboxRecordFactory` и `OutboxBusNames` —
  останется по одному;
- префикс шины в `OutboxDbContext`, `OutboxDeliveryBusName` и проверку «один сервис — одна шина отправки»
  (`TransactionalOutbox.EntityFrameworkCore/DependencyInjection.cs`);
- явный `IUnitOfWorkOutbox` там, где он нужен только из-за второй шины (`NotificationPublisher`,
  `EdoNotificationEmailService`); `AddExplicitUnitOfWorkOutbox` останется только для десктопа;
- раздел «Вторая шина» и таблицу сервисов-отправителей по vhost'ам в этом документе.

В коде такие места помечены комментариями про переход на один vhost (поиск по «один vhost»).

Отдельной задачей можно вынести отправку в один сервис-ретранслятор (после перехода на один vhost — один на всё):
остальные сервисы и десктоп только пишут строки, служба отправки MassTransit работает только в ретрансляторе.
Это убирает опрос таблиц из каждого сервиса и связанность сервисов через отправку. Запись сообщений не меняется:
в остальных сервисах убирается только служба отправки (`UseBusOutbox` в `AddVodovozOutbox`); есть ли в MassTransit 8.0.12
штатный способ MassTransit её выключить, оставив запись, — проверить при реализации.
