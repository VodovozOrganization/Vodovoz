using EmailPrepareWorker.SendEmailMessageBuilders;
using RabbitMQ.MailSending;

namespace EmailPrepareWorker.Prepares
{
	public interface IEmailSendMessagePreparer
	{
		SendEmailMessage PrepareMessage(SendEmailMessageBuilder builder, string connectionString);
	}
}
