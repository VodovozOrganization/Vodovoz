using EmailPrepareWorker.SendEmailMessageBuilders;
using RabbitMQ.MailSending;

namespace EmailPrepareWorker.Prepares
{
	public class EmailSendMessagePreparer : IEmailSendMessagePreparer
	{
		public SendEmailMessage PrepareMessage(SendEmailMessageBuilder builder, string connectionString)
		{
			return builder
				.AddFromContact()
				.AddToContact()
				.AddTemplate()
				.AddAttachment(connectionString)
				.AddPayload();
		}
	}
}
