using System;
using System.Globalization;
using System.Linq;
using Vodovoz.Settings.Notifications;

namespace Vodovoz.Settings.Database.Notifications
{
	public class BitrixNotificationsSendSettings : IBitrixNotificationsSendSettings
	{
		private readonly ISettingsController _settingsController;
		private readonly string _parametersPrefix = "BitrixNotifications.";
		private const char _stageIdsSeparator = ',';
		private const string _dateSettingFormat = "yyyy-MM-dd";

		public BitrixNotificationsSendSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int[] CashlessDebtsOrganizations =>
			_settingsController.GetStringValue($"{_parametersPrefix}CashlessDebtsOrganizations")
			.FromStringToIntArray();

		public TimeSpan CashlessDebtsNotificationsSendInterval =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}CashlessDebtsNotificationsSendInterval");

		public string BitrixBaseUrl =>
			_settingsController.GetStringValue($"{_parametersPrefix}BitrixBaseUrl");

		public string BitrixToken =>
			_settingsController.GetStringValue($"{_parametersPrefix}BitrixToken");

		public TimeSpan ClientTimeout =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}ClientTimeout");

		public string BitrixDealsBaseUrl =>
			_settingsController.GetStringValue($"{_parametersPrefix}BitrixDealsBaseUrl");

		public string BitrixDealsUser =>
			_settingsController.GetStringValue($"{_parametersPrefix}BitrixDealsUser");

		public string BitrixDealsToken =>
			_settingsController.GetStringValue($"{_parametersPrefix}BitrixDealsToken");

		public TimeSpan PlannedOrdersNotificationsSendInterval =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}PlannedOrdersNotificationsSendInterval");

		public TimeSpan PlannedOrdersSendTimeFrom =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}PlannedOrdersSendTimeFrom");

		public TimeSpan PlannedOrdersSendTimeTo =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}PlannedOrdersSendTimeTo");

		public int PlannedOrdersDaysToNextOrderAfterSingleOrder =>
			_settingsController.GetIntValue($"{_parametersPrefix}PlannedOrdersDaysAfterSingleOrder");

		public bool PlannedOrdersNotificationsSendEnabled =>
			_settingsController.GetBoolValue($"{_parametersPrefix}PlannedOrdersNotificationsSendEnabled");

		public TimeSpan LastServiceOrdersNotificationsSendInterval =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}LastServiceOrdersSendInterval");

		public TimeSpan LastServiceOrdersSendTimeFrom =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}LastServiceOrdersSendTimeFrom");

		public TimeSpan LastServiceOrdersSendTimeTo =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}LastServiceOrdersSendTimeTo");

		public bool LastServiceOrdersNotificationsSendEnabled =>
			_settingsController.GetBoolValue($"{_parametersPrefix}LastServiceOrdersSendEnabled");

		public int LastServiceOrdersMinDaysSinceLastOrder =>
			_settingsController.GetIntValue($"{_parametersPrefix}LastServiceOrdersMinDaysSinceLastOrder");

		public DateTime LastServiceOrdersMinDeliveryDate =>
			_settingsController.GetDateTimeValue($"{_parametersPrefix}LastServiceOrdersMinDeliveryDate");

		public TimeSpan UndeliveredOrdersSendInterval =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}UndeliveredOrdersSendInterval");

		public bool UndeliveredOrdersSendEnabled =>
			_settingsController.GetBoolValue($"{_parametersPrefix}UndeliveredOrdersSendEnabled");

		public DateTime UndeliveredOrdersMinLastEditedTime =>
			_settingsController.GetDateTimeValue($"{_parametersPrefix}UndeliveredOrdersMinLastEditedTime");

		public string PlannedOrdersNewStageId =>
			_settingsController.GetStringValue($"{_parametersPrefix}PlannedOrdersNewStageId");

		public string PlannedOrdersCompletedStageId =>
			_settingsController.GetStringValue($"{_parametersPrefix}PlannedOrdersCompletedStageId");

		public string[] PlannedOrdersFinalStageIds =>
			_settingsController.GetStringValue($"{_parametersPrefix}PlannedOrdersFinalStageIds")
			?.Split(_stageIdsSeparator)
			.Select(x => x.Trim())
			.Where(x => !string.IsNullOrWhiteSpace(x))
			.ToArray()
			?? Array.Empty<string>();

		public DateTime PlannedOrdersLastOrdersCheckDate =>
			_settingsController.GetDateTimeValue($"{_parametersPrefix}PlannedOrdersLastOrdersCheckDate");

		public void UpdatePlannedOrdersLastOrdersCheckDate(DateTime lastOrdersCheckDate) =>
			_settingsController.CreateOrUpdateSetting(
				$"{_parametersPrefix}PlannedOrdersLastOrdersCheckDate",
				lastOrdersCheckDate.ToString(_dateSettingFormat, CultureInfo.InvariantCulture));
	}
}
