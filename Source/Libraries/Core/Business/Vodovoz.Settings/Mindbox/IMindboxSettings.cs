namespace Vodovoz.Settings.Mindbox
{
	/// <summary>
	/// Настройки интеграции с Mindbox
	/// </summary>
	public interface IMindboxSettings
	{
		/// <summary>
		/// Признак включенной интеграции
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Базовый адрес API Mindbox, например https://api.mindbox.ru
		/// </summary>
		string BaseUrl { get; }

		/// <summary>
		/// Секретный ключ проекта Mindbox
		/// </summary>
		string SecretKey { get; }

		/// <summary>
		/// Идентификатор точки доступа Mindbox
		/// </summary>
		string EndpointId { get; }

		/// <summary>
		/// Системное имя операции создания контрагента
		/// </summary>
		string CreateCounterpartyOperation { get; }

		/// <summary>
		/// Тип операции создания контрагента: sync или async
		/// </summary>
		string CreateCounterpartyOperationType { get; }

		/// <summary>
		/// Системное имя операции массового импорта клиентов
		/// </summary>
		string ImportOperation { get; }

		/// <summary>
		/// Системное имя сегмента для массового импорта
		/// </summary>
		string ImportSegment { get; }

		/// <summary>
		/// Действие регистрации клиента для массового импорта
		/// </summary>
		string ImportSourceActionTemplate { get; }

		/// <summary>
		/// Размер пакета строк CSV при массовом импорте
		/// </summary>
		int ImportBatchSize { get; }

		/// <summary>
		/// Максимальное количество сообщений очереди, обрабатываемых за один цикл
		/// </summary>
		int SendBatchSize { get; }

		/// <summary>
		/// Максимальное количество попыток отправки сообщения
		/// </summary>
		int MaxSendAttempts { get; }

		/// <summary>
		/// Базовая задержка перед повторной отправкой, секунды
		/// </summary>
		int RetryDelaySeconds { get; }
	}
}
