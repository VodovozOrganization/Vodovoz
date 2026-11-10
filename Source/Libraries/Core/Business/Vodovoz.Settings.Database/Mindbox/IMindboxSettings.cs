namespace Vodovoz.Settings.Database.Mindbox
{
	/// <summary>
	/// Настройки интеграции с Mindbox, хранящиеся в base_parameters
	/// </summary>
	public interface IMindboxSettings
	{
		/// <summary>
		/// Признак включённости интеграции с Mindbox
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Базовый URL API Mindbox
		/// </summary>
		string BaseUrl { get; }

		/// <summary>
		/// Секретный ключ для доступа к API Mindbox
		/// </summary>
		string SecretKey { get; }

		/// <summary>
		/// Идентификатор эндпоинта Mindbox
		/// </summary>
		string EndpointId { get; }

		/// <summary>
		/// Название операции создания клиента
		/// </summary>
		string CreateCustomerOperation { get; }

		/// <summary>
		/// Тип операции создания клиента
		/// </summary>
		string CreateCustomerOperationType { get; }

		/// <summary>
		/// Идентификатор точки контакта для контрагентов, созданных в ERP
		/// </summary>
		string PointOfContactErp { get; }

		/// <summary>
		/// Таймаут запросов к Mindbox в секундах
		/// </summary>
		int TimeoutSeconds { get; }

		/// <summary>
		/// Размер батча при отправке данных
		/// </summary>
		int SendBatchSize { get; }

		/// <summary>
		/// Размер батча при постановке в очередь
		/// </summary>
		int EnqueueBatchSize { get; }

		/// <summary>
		/// Расписание повторов в минутах (например, "1,5,15,60")
		/// </summary>
		string RetryScheduleMinutes { get; }

		/// <summary>
		/// Интервал повторов в минутах
		/// </summary>
		int RetryIntervalMinutes { get; }

		/// <summary>
		/// Максимальное время повторов в часах
		/// </summary>
		int RetryMaxHours { get; }

		/// <summary>
		/// Длительность паузы в минутах
		/// </summary>
		int PauseMinutes { get; }

		/// <summary>
		/// Количество временных ошибок до включения паузы
		/// </summary>
		int TransientErrorsBeforePause { get; }

		/// <summary>
		/// Интервал оповещения о недоступности в минутах
		/// </summary>
		int UnavailableAlertMinutes { get; }

		/// <summary>
		/// Признак включённости расчёта бонусов в Mindbox
		/// </summary>
		bool BonusCalculationEnabled { get; }

		/// <summary>
		/// Название операции расчёта цен в каталоге и на карточке продукта
		/// </summary>
		string CalculateProductListOperation { get; }

		/// <summary>
		/// Имя идентификатора товара в запросе к Mindbox
		/// </summary>
		string BonusItemIdName { get; }

		/// <summary>
		/// Системное имя балльного счёта, по которому берутся бонусы
		/// </summary>
		string BonusBalanceSystemName { get; }

		/// <summary>
		/// Размер пачки позиций при расчёте бонусов
		/// </summary>
		int BonusBatchSize { get; }

		/// <summary>
		/// Максимальное количество одновременно отправляемых пачек
		/// </summary>
		int BonusMaxParallelBatches { get; }

		/// <summary>
		/// Таймаут одной пачки при расчёте бонусов в секундах
		/// </summary>
		int BonusBatchTimeoutSeconds { get; }

		/// <summary>
		/// Общий лимит ожидания Mindbox при расчёте бонусов в миллисекундах
		/// </summary>
		int BonusTotalTimeoutMilliseconds { get; }

		/// <summary>
		/// Количество подряд идущих ошибок до паузы при расчёте бонусов
		/// </summary>
		int BonusErrorsBeforePause { get; }

		/// <summary>
		/// Длительность паузы при расчёте бонусов в минутах
		/// </summary>
		int BonusPauseMinutes { get; }

		/// <summary>
		/// Время хранения ответа Mindbox в кеше в секундах. 0 — кеш выключен
		/// </summary>
		int BonusCacheSeconds { get; }

		/// <summary>
		/// Идентификатор последнего поставленного в очередь контрагента
		/// </summary>
		int LastEnqueuedCounterpartyId { get; }

		/// <summary>
		/// Название операции расчёта бонусов при оформлении заказа
		/// </summary>
		string CalculateAuthorizedCartOperation { get; }

		/// <summary>
		/// Название операции создания заказа с бонусами
		/// </summary>
		string CreateAuthorizedLoyaltyOrderOperation { get; }

		/// <summary>
		/// Таймаут запроса CalculateAuthorizedCart в секундах
		/// </summary>
		int AuthorizedCartTimeoutSeconds { get; }

		/// <summary>
		/// Таймаут запроса CreateAuthorizedLoyaltyOrder в секундах
		/// </summary>
		int AuthorizedLoyaltyOrderTimeoutSeconds { get; }

		/// <summary>
		/// Имя идентификатора заказа в Mindbox: externalID или erptestOrderID
		/// </summary>
		string MindboxOrderIdName { get; }

		/// <summary>
		/// Режим бонусной программы: Disabled, Clients, All
		/// </summary>
		string BonusProgramMode { get; }

		/// <summary>
		/// Список идентификаторов клиентов бонусной программы через запятую
		/// </summary>
		string BonusProgramClientIds { get; }

		/// <summary>
		/// Устанавливает идентификатор последнего поставленного в очередь контрагента
		/// </summary>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		void SetLastEnqueuedCounterpartyId(int counterpartyId);
	}
}
