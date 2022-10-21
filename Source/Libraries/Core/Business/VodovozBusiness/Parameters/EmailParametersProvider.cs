using System;

namespace Vodovoz.Parameters
{
	public class EmailParametersProvider : IEmailParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public EmailParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public string DefaultEmailSenderName => _parametersProvider.GetStringValue("email_sender_name");
		public string DefaultEmailSenderAddress => _parametersProvider.GetStringValue("email_for_email_delivery");
		public string DocumentEmailSenderName => DefaultEmailSenderName;
		public string DocumentEmailSenderAddress => DefaultEmailSenderAddress;
		public string InvalidSignatureNotificationEmailAddress =>
			_parametersProvider.GetStringValue("invalid_signature_notification_email");
		public string UnsubscribeUrl => _parametersProvider.GetStringValue("bulk_email_unsubscribe_url");
		public int BulkEmailEventOtherReasonId => _parametersProvider.GetIntValue("bulk_email_event_other_reason_id");
		public int BulkEmailEventOperatorReasonId => _parametersProvider.GetIntValue("bulk_email_event_operator_reason_id");
	}
}
