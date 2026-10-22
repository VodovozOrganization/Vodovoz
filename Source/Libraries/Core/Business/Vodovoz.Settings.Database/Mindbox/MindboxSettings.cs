using Vodovoz.Settings.Mindbox;

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
		private const string CreateCounterpartyOperationSetting = "Mindbox.CreateCounterpartyOperation";
		private const string CreateCounterpartyOperationTypeSetting = "Mindbox.CreateCounterpartyOperationType";
		private const string ImportOperationSetting = "Mindbox.ImportOperation";
		private const string ImportSegmentSetting = "Mindbox.ImportSegment";
		private const string ImportSourceActionTemplateSetting = "Mindbox.ImportSourceActionTemplate";
		private const string ImportBatchSizeSetting = "Mindbox.ImportBatchSize";
		private const string SendBatchSizeSetting = "Mindbox.SendBatchSize";
		private const string MaxSendAttemptsSetting = "Mindbox.MaxSendAttempts";
		private const string RetryDelaySecondsSetting = "Mindbox.RetryDelaySeconds";

		private readonly ISettingsController _settingsController;

		public MindboxSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
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
		public string CreateCounterpartyOperation => GetRequiredString(CreateCounterpartyOperationSetting);

		/// <inheritdoc />
		public string CreateCounterpartyOperationType => GetOptionalString(CreateCounterpartyOperationTypeSetting, "async");

		/// <inheritdoc />
		public string ImportOperation => GetOptionalString(ImportOperationSetting, "DirectCrm.Customers.Import");

		/// <inheritdoc />
		public string ImportSegment => GetOptionalString(ImportSegmentSetting, string.Empty);

		/// <inheritdoc />
		public string ImportSourceActionTemplate => GetOptionalString(ImportSourceActionTemplateSetting, string.Empty);

		/// <inheritdoc />
		public int ImportBatchSize => GetOptionalInt(ImportBatchSizeSetting, 20000);

		/// <inheritdoc />
		public int SendBatchSize => GetOptionalInt(SendBatchSizeSetting, 50);

		/// <inheritdoc />
		public int MaxSendAttempts => GetOptionalInt(MaxSendAttemptsSetting, 10);

		/// <inheritdoc />
		public int RetryDelaySeconds => GetOptionalInt(RetryDelaySecondsSetting, 60);

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
}
