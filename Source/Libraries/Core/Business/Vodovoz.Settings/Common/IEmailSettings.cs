namespace Vodovoz.Settings.Common
{
	public interface IEmailSettings
	{
		string DefaultEmailSenderAddress { get; }
		string DefaultEmailSenderName { get; }
		string DocumentEmailSenderAddress { get; }
		string EmailSenderAddressForUpd { get; }

		string DocumentEmailSenderName { get; }
		string InvalidSignatureNotificationEmailAddress { get; }
		string UnsubscribeUrl { get; }
		int BulkEmailEventOtherReasonId { get; }
		int BulkEmailEventOperatorReasonId { get; }
		int EmailTypeForReceiptsId { get; }
	}
}
