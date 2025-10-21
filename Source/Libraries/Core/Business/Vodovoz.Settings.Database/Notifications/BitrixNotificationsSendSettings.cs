using System;
using Vodovoz.Settings.Notifications;

namespace Vodovoz.Settings.Database.Notifications
{
	public class BitrixNotificationsSendSettings : IBitrixNotificationsSendSettings
	{
		private readonly ISettingsController _settingsController;
		private readonly string _parametersPrefix = "BitrixNotifications.";

		public BitrixNotificationsSendSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public TimeSpan CashlessDebtsNotificationsSendInterval =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}CashlessDebtsNotificationsSendInterval");

		public string BitrixBaseUrl =>
			_settingsController.GetStringValue($"{_parametersPrefix}BitrixBaseUrl");

		public string BitrixToken =>
			_settingsController.GetStringValue($"{_parametersPrefix}BitrixToken");
	}
}
