using System.Text.Json.Serialization;

namespace Mailjet.Api.Abstractions.Events
{
	public class MailUnsubscribeEvent : MailEvent
	{
		[JsonPropertyName("mj_list_id")]
		public int MailjetListId { get; set; }
		[JsonPropertyName("ip")]
		public string IpAddress { get; set; }
		[JsonPropertyName("geo")]
		public string Geo { get; set; } // Вроде это ISO коды, если вдруг понадобится уточнение - надо копать глубже
		[JsonPropertyName("agent")]
		public string Agent { get; set; }
	}
}
