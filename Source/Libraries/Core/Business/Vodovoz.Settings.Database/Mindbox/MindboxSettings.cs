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
		private const string PointOfContactErpSetting = "Mindbox.PointOfContactErp";
		private const string TimeoutSecondsSetting = "Mindbox.TimeoutSeconds";
		private const string SendBatchSizeSetting = "Mindbox.SendBatchSize";
		private const string EnqueueBatchSizeSetting = "Mindbox.EnqueueBatchSize";
		private const string RetryScheduleMinutesSetting = "Mindbox.RetryScheduleMinutes";
		private const string RetryIntervalMinutesSetting = "Mindbox.RetryIntervalMinutes";
		private const string RetryMaxHoursSetting = "Mindbox.RetryMaxHours";
		private const string PauseMinutesSetting = "Mindbox.PauseMinutes";
		private const string TransientErrorsBeforePauseSetting = "Mindbox.TransientErrorsBeforePause";
		private const string UnavailableAlertMinutesSetting = "Mindbox.UnavailableAlertMinutes";
		private const string BonusCalculationEnabledSetting = "Mindbox.BonusCalculationEnabled";
		private const string CalculateProductListOperationSetting = "Mindbox.CalculateProductListOperation";
		private const string BonusItemIdNameSetting = "Mindbox.BonusItemIdName";
		private const string BonusBalanceSystemNameSetting = "Mindbox.BonusBalanceSystemName";
		private const string BonusBatchSizeSetting = "Mindbox.BonusBatchSize";
		private const string BonusMaxParallelBatchesSetting = "Mindbox.BonusMaxParallelBatches";
		private const string BonusBatchTimeoutSecondsSetting = "Mindbox.BonusBatchTimeoutSeconds";
		private const string BonusTotalTimeoutMillisecondsSetting = "Mindbox.BonusTotalTimeoutMilliseconds";
		private const string BonusErrorsBeforePauseSetting = "Mindbox.BonusErrorsBeforePause";
		private const string BonusPauseMinutesSetting = "Mindbox.BonusPauseMinutes";
		private const string BonusCacheSecondsSetting = "Mindbox.BonusCacheSeconds";
		private const string LastEnqueuedCounterpartyIdSetting = "Mindbox.LastEnqueuedCounterpartyId";
		private const string CalculateAuthorizedCartOperationSetting = "Mindbox.CalculateAuthorizedCartOperation";
		private const string AuthorizedCartTimeoutSecondsSetting = "Mindbox.AuthorizedCartTimeoutSeconds";
		private const string CreateAuthorizedLoyaltyOrderOperationSetting = "Mindbox.CreateAuthorizedLoyaltyOrderOperation";
		private const string AuthorizedLoyaltyOrderTimeoutSecondsSetting = "Mindbox.AuthorizedLoyaltyOrderTimeoutSeconds";
		private const string MindboxOrderIdNameSetting = "Mindbox.MindboxOrderIdName";
		private const string UpdateOrderLinesStatusOperationSetting = "Mindbox.UpdateOrderLinesStatusOperation";
		private const string GetOrderInfoOperationSetting = "Mindbox.GetOrderInfoOperation";
		private const string GetBalanceHistoryOperationSetting = "Mindbox.GetBalanceHistoryOperation";
		private const string OrderLineStatusMappingSetting = "Mindbox.OrderLineStatusMapping";
		private const string BonusProgramModeSetting = "Mindbox.BonusProgramMode";
		private const string BonusProgramClientIdsSetting = "Mindbox.BonusProgramClientIds";

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
		public string BaseUrl => GetOptionalString(BaseUrlSetting, "https://api.s.mindbox.ru");

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
		public string PointOfContactErp =>
			GetOptionalString(PointOfContactErpSetting, "1");

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
		public bool BonusCalculationEnabled =>
			GetOptionalBool(BonusCalculationEnabledSetting, false);

		/// <inheritdoc />
		public string CalculateProductListOperation =>
			GetOptionalString(CalculateProductListOperationSetting, "CalculateProductList");

		/// <inheritdoc />
		public string BonusItemIdName =>
			GetOptionalString(BonusItemIdNameSetting, "itemErpId");

		/// <inheritdoc />
		public string BonusBalanceSystemName =>
			GetOptionalString(BonusBalanceSystemNameSetting, string.Empty);

		/// <inheritdoc />
		public int BonusBatchSize => GetOptionalInt(BonusBatchSizeSetting, 100);

		/// <inheritdoc />
		public int BonusMaxParallelBatches => GetOptionalInt(BonusMaxParallelBatchesSetting, 5);

		/// <inheritdoc />
		public int BonusBatchTimeoutSeconds => GetOptionalInt(BonusBatchTimeoutSecondsSetting, 2);

		/// <inheritdoc />
		public int BonusTotalTimeoutMilliseconds => GetOptionalInt(BonusTotalTimeoutMillisecondsSetting, 2500);

		/// <inheritdoc />
		public int BonusErrorsBeforePause => GetOptionalInt(BonusErrorsBeforePauseSetting, 5);

		/// <inheritdoc />
		public int BonusPauseMinutes => GetOptionalInt(BonusPauseMinutesSetting, 1);

		/// <inheritdoc />
		public int BonusCacheSeconds => GetOptionalInt(BonusCacheSecondsSetting, 0);

		/// <inheritdoc />
		public int LastEnqueuedCounterpartyId =>
			GetOptionalInt(LastEnqueuedCounterpartyIdSetting, -1);

		/// <inheritdoc />
		public string CalculateAuthorizedCartOperation =>
			GetOptionalString(CalculateAuthorizedCartOperationSetting, "CalculateAuthorizedCart");

		/// <inheritdoc />
		public string CreateAuthorizedLoyaltyOrderOperation =>
			GetOptionalString(CreateAuthorizedLoyaltyOrderOperationSetting, "CreateAuthorizedLoyaltyOrder");

		/// <inheritdoc />
		public int AuthorizedCartTimeoutSeconds =>
			GetOptionalInt(AuthorizedCartTimeoutSecondsSetting, 3);

		/// <inheritdoc />
		public int AuthorizedLoyaltyOrderTimeoutSeconds =>
			GetOptionalInt(AuthorizedLoyaltyOrderTimeoutSecondsSetting, 10);

		/// <inheritdoc />
		public string MindboxOrderIdName =>
			GetOptionalString(MindboxOrderIdNameSetting, "externalID");

		/// <inheritdoc />
		public string UpdateOrderLinesStatusOperation =>
			GetOptionalString(UpdateOrderLinesStatusOperationSetting, "UpdateOrderLinesStatus");

		/// <inheritdoc />
		public string GetOrderInfoOperation =>
			GetOptionalString(GetOrderInfoOperationSetting, "GetOrderInfo");

		/// <inheritdoc />
		public string GetBalanceHistoryOperation =>
			GetOptionalString(GetBalanceHistoryOperationSetting, "GetBalanceHistory");

		/// <inheritdoc />
		public string OrderLineStatusMapping =>
			GetOptionalString(
				OrderLineStatusMappingSetting,
				"{\"OrderPerformed\":\"Created\",\"WaitingForPayment\":\"WaitingForPayment\",\"OrderCollecting\":\"OrderCollecting\",\"OrderDelivering\":\"OrderDelivering\",\"OrderCompleted\":\"OrderCompleted\",\"Canceled\":\"Canceled\",\"Paid\":\"Paid\"}");

		/// <inheritdoc />
		public string BonusProgramMode =>
			GetOptionalString(BonusProgramModeSetting, "Disabled");

		/// <inheritdoc />
		public string BonusProgramClientIds =>
			GetOptionalString(BonusProgramClientIdsSetting, string.Empty);

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

		private bool GetOptionalBool(string settingName, bool defaultValue)
		{
			if(!_settingsController.ContainsSetting(settingName))
			{
				return defaultValue;
			}

			try
			{
				return _settingsController.GetBoolValue(settingName);
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
