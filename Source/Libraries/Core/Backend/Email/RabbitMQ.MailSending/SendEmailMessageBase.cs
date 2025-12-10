using Mailjet.Api.Abstractions;
using System.Text.Json;

namespace RabbitMQ.MailSending
{
	public class SendEmailMessageBase : EmailMessage
	{
		public EmailPayload Payload
		{
			get => JsonSerializer.Deserialize<EmailPayload>(EventPayload);
			set { EventPayload = JsonSerializer.Serialize(value); }
		}
	}
}
