using System.Text.Json.Serialization;

namespace Mailjet.Api.Abstractions.Events
{
	public class MailBounceEvent : MailEvent
	{
		public override MailEventType EventType => MailEventType.bounce;

		[JsonPropertyName("blocked")]
		public bool Blocked { get; set; }
		[JsonPropertyName("hard_bounce")]
		public bool HardBounce { get; set; }
		[JsonPropertyName("error_related_to")]
		public string ErrorRelatedTo { get; set; }
		[JsonPropertyName("error")]
		public string Error { get; set; }
		[JsonPropertyName("comment")]
		public string Comment { get; set; }
	}
}
