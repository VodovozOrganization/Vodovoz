using System.Text.Json.Serialization;

namespace Mailjet.Api.Abstractions.Events
{
	public class MailSpamEvent : MailEvent
	{
		public override MailEventType EventType => MailEventType.spam;

		[JsonPropertyName("source")]
		public string Source { get; set; }
	}
}
