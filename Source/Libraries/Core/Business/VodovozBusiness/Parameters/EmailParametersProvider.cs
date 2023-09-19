using System;
using Vodovoz.Settings;

namespace Vodovoz.Parameters
{
	public class EmailParametersProvider : IEmailParametersProvider
	{
		private readonly ISettingsController _settingsController;

		public EmailParametersProvider(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string DefaultEmailSenderName => _settingsController.GetStringValue("email_sender_name");
		public string DefaultEmailSenderAddress => _settingsController.GetStringValue("email_for_email_delivery");
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
