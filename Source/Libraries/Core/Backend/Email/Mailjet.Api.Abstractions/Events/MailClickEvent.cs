using System.Text.Json.Serialization;

namespace Mailjet.Api.Abstractions.Events
{
	public class MailClickEvent : MailEvent
	{
		public override MailEventType EventType => MailEventType.click;

		[JsonPropertyName("url")]
		public string Url { get; set; }
		[JsonPropertyName("ip")]
		public string IpAddress { get; set; }
		[JsonPropertyName("geo")]
		public string Geo { get; set; } // Вроде это ISO коды, если вдруг понадобится уточнение - надо копать глубже
		[JsonPropertyName("agent")]
		public string Agent { get; set; }
	}
}
