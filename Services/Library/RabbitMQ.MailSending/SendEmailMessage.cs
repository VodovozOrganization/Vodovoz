using Mailjet.Api.Abstractions;
using System.Text.Json;

namespace RabbitMQ.MailSending
{
	public class SendEmailMessage : EmailMessage
	{
		public EmailPayload Payload
		{
			get => JsonSerializer.Deserialize<EmailPayload>(EventPayload);
			set => JsonSerializer.Serialize(value);
		}
	}
}
