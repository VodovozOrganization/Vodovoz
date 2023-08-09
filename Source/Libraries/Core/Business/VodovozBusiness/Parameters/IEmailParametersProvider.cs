namespace Vodovoz.Parameters
{
	public interface IEmailParametersProvider
	{
		string DefaultEmailSenderAddress { get; }
		string DefaultEmailSenderName { get; }
		string DocumentEmailSenderAddress { get; }
		string DocumentEmailSenderName { get; }
		string InvalidSignatureNotificationEmailAddress { get; }
		string UnsubscribeUrl { get; }
		int BulkEmailEventOtherReasonId { get; }
		int BulkEmailEventOperatorReasonId { get; }
		int EmailTypeForReceiptsId { get; }
	}
}
