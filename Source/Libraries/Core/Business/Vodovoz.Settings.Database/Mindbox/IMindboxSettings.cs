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
		/// Идентификатор последнего поставленного в очередь контрагента
		/// </summary>
		int LastEnqueuedCounterpartyId { get; }

		/// <summary>
		/// Устанавливает идентификатор последнего поставленного в очередь контрагента
		/// </summary>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		void SetLastEnqueuedCounterpartyId(int counterpartyId);
	}
}
