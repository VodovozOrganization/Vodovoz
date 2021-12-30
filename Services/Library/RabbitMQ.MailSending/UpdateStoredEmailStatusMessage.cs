using Mailjet.Api.Abstractions.Events;
using System;

namespace RabbitMQ.MailSending
{
	public class UpdateStoredEmailStatusMessage
	{
		public EmailPayload EventPayload { get; set; }
		public MailEventType Status { get; set; }
		public string MailjetMessageId { get; set; }
		public DateTime RecievedAt { get; set; }
		public string ErrorInfo { get; set; }
	}
}
