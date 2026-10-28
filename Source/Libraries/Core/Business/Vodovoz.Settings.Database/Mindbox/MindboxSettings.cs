using System;
using System.Globalization;

namespace Vodovoz.Settings.Database.Mindbox
{
	/// <summary>
	/// Настройки интеграции с Mindbox, хранящиеся в base_parameters
	/// </summary>
	public class MindboxSettings : IMindboxSettings
	{
		private const string IsEnabledSetting = "Mindbox.IsEnabled";
		private const string BaseUrlSetting = "Mindbox.BaseUrl";
		private const string SecretKeySetting = "Mindbox.SecretKey";
		private const string EndpointIdSetting = "Mindbox.EndpointId";
		private const string CreateCustomerOperationSetting = "Mindbox.CreateCustomerOperation";
		private const string CreateCustomerOperationTypeSetting = "Mindbox.CreateCustomerOperationType";
		private const string TimeoutSecondsSetting = "Mindbox.TimeoutSeconds";
		private const string SendBatchSizeSetting = "Mindbox.SendBatchSize";
		private const string EnqueueBatchSizeSetting = "Mindbox.EnqueueBatchSize";
		private const string RetryScheduleMinutesSetting = "Mindbox.RetryScheduleMinutes";
		private const string RetryIntervalMinutesSetting = "Mindbox.RetryIntervalMinutes";
		private const string RetryMaxHoursSetting = "Mindbox.RetryMaxHours";
		private const string PauseMinutesSetting = "Mindbox.PauseMinutes";
		private const string TransientErrorsBeforePauseSetting = "Mindbox.TransientErrorsBeforePause";
		private const string UnavailableAlertMinutesSetting = "Mindbox.UnavailableAlertMinutes";
		private const string LastEnqueuedCounterpartyIdSetting = "Mindbox.LastEnqueuedCounterpartyId";

		private readonly ISettingsController _settingsController;

		public MindboxSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		/// <inheritdoc />
		public bool IsEnabled
		{
			get
			{
				if(!_settingsController.ContainsSetting(IsEnabledSetting))
				{
					return false;
				}

				try
				{
					return _settingsController.GetBoolValue(IsEnabledSetting);
				}
				catch(SettingException)
				{
					return false;
				}
			}
		}

		/// <inheritdoc />
		public string BaseUrl => GetOptionalString(BaseUrlSetting, "https://api.mindbox.ru");

		/// <inheritdoc />
		public string SecretKey => GetRequiredString(SecretKeySetting);

		/// <inheritdoc />
		public string EndpointId => GetRequiredString(EndpointIdSetting);

		/// <inheritdoc />
		public string CreateCustomerOperation => GetRequiredString(CreateCustomerOperationSetting);

		/// <inheritdoc />
		public string CreateCustomerOperationType =>
			GetOptionalString(CreateCustomerOperationTypeSetting, "sync");

		/// <inheritdoc />
		public int TimeoutSeconds => GetOptionalInt(TimeoutSecondsSetting, 10);

		/// <inheritdoc />
		public int SendBatchSize => GetOptionalInt(SendBatchSizeSetting, 50);

		/// <inheritdoc />
		public int EnqueueBatchSize => GetOptionalInt(EnqueueBatchSizeSetting, 1000);

		/// <inheritdoc />
		public string RetryScheduleMinutes =>
			GetOptionalString(RetryScheduleMinutesSetting, "1,5,15,60");

		/// <inheritdoc />
		public int RetryIntervalMinutes => GetOptionalInt(RetryIntervalMinutesSetting, 60);

		/// <inheritdoc />
		public int RetryMaxHours => GetOptionalInt(RetryMaxHoursSetting, 24);

		/// <inheritdoc />
		public int PauseMinutes => GetOptionalInt(PauseMinutesSetting, 5);

		/// <inheritdoc />
		public int TransientErrorsBeforePause =>
			GetOptionalInt(TransientErrorsBeforePauseSetting, 3);

		/// <inheritdoc />
		public int UnavailableAlertMinutes => GetOptionalInt(UnavailableAlertMinutesSetting, 15);

		/// <inheritdoc />
		public int LastEnqueuedCounterpartyId =>
			GetOptionalInt(LastEnqueuedCounterpartyIdSetting, -1);

		/// <inheritdoc />
		public void SetLastEnqueuedCounterpartyId(int counterpartyId)
		{
			_settingsController.CreateOrUpdateSetting(
				LastEnqueuedCounterpartyIdSetting,
				counterpartyId.ToString(CultureInfo.InvariantCulture),
				TimeSpan.FromMinutes(1));
		}

		private string GetRequiredString(string settingName)
		{
			if(!_settingsController.ContainsSetting(settingName))
			{
				throw new SettingException($"В базе данных не добавлена настройка ({settingName})");
			}

			return _settingsController.GetStringValue(settingName);
		}

		private string GetOptionalString(string settingName, string defaultValue)
		{
			if(!_settingsController.ContainsSetting(settingName))
			{
				return defaultValue;
			}

			try
			{
				return _settingsController.GetStringValue(settingName);
			}
			catch(SettingException)
			{
				return defaultValue;
			}
		}

		private int GetOptionalInt(string settingName, int defaultValue)
		{
			if(!_settingsController.ContainsSetting(settingName))
			{
				return defaultValue;
			}

			try
			{
				return _settingsController.GetIntValue(settingName);
			}
			catch(SettingException)
			{
				return defaultValue;
			}
		}
	}

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
