using EmailPrepareWorker.SendEmailMessageBuilders;
using System.Text;
using System.Text.Json;
using RabbitMQ.MailSending;

namespace EmailPrepareWorker.Prepares
{
	public class EmailSendMessagePreparer : IEmailSendMessagePreparer
	{
		public byte[] PrepareMessage(SendEmailMessageBuilder builder, string connectionString)
		{
			SendEmailMessage message = builder
				.AddFromContact()
				.AddToContact()
				.AddTemplate()
				.AddAttachment(connectionString)
				.AddPayload();

			var serializedMessage = JsonSerializer.Serialize(message);
			var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

			return sendingBody;
		}
	}
}
