using System;
using Vodovoz.Services;

namespace Vodovoz.Settings.Database.Sms
{
	public class SmsNotifierSettings : ISmsNotifierSettings
	{
		private readonly ISettingsController _settingsController;

		public SmsNotifierSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public bool IsSmsNotificationsEnabled => _settingsController.GetBoolValue("is_sms_notification_enabled");

		public string NewClientSmsTextTemplate => _settingsController.GetStringValue("new_client_sms_text_template");

		public decimal LowBalanceLevel => _settingsController.GetDecimalValue("low_balance_level_for_sms_notifications");

		public string LowBalanceNotifiedPhone => _settingsController.GetStringValue("low_balance_sms_notified_phone");

		public string LowBalanceNotifyText => _settingsController.GetStringValue("low_balance_sms_notify_text");

		public string UndeliveryAutoTransferNotApprovedTextTemplate => _settingsController.GetStringValue("undelivery_autotransport_notapproved_sms_text_template");
	}
}
