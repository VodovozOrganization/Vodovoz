using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;

namespace Vodovoz.Settings.Database.Common
{
	public class EmailSettings : IEmailSettings
	{
		private readonly ISettingsController _settingsController;

		public EmailSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string DefaultEmailSenderName => _settingsController.GetStringValue("email_sender_name");
		public string DefaultEmailSenderAddress => _settingsController.GetStringValue("email_for_email_delivery");
		public string EmailSenderAddressForUpd => _settingsController.GetStringValue("email_sender_address_for_upd");
		public string DocumentEmailSenderName => DefaultEmailSenderName;
		public string DocumentEmailSenderAddress => DefaultEmailSenderAddress;
		public string InvalidSignatureNotificationEmailAddress =>
			_settingsController.GetStringValue("invalid_signature_notification_email");
		public string UnsubscribeUrl => _settingsController.GetStringValue("bulk_email_unsubscribe_url");
		public int BulkEmailEventOtherReasonId => _settingsController.GetIntValue("bulk_email_event_other_reason_id");
		public int BulkEmailEventOperatorReasonId => _settingsController.GetIntValue("bulk_email_event_operator_reason_id");
		public int EmailTypeForReceiptsId => _settingsController.GetIntValue(nameof(EmailTypeForReceiptsId));
	}
}
