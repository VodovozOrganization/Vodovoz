using EmailPrepareWorker.SendEmailMessageBuilders;

namespace EmailPrepareWorker.Prepares
{
	public interface IEmailSendMessagePreparer
	{
		byte[] PrepareMessage(SendEmailMessageBuilder builder, string connectionString);
	}
}
