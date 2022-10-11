using System.Text.Json.Serialization;

namespace Mailjet.Api.Abstractions.Events
{
	public class MailEvent
	{
		[JsonPropertyName("event")]
		public virtual MailEventType EventType { get; set; }
		[JsonPropertyName("time")]
		public long Time { get; set; }
		[JsonPropertyName("MessageID")]
		public long MessageId { get; set; }
		[JsonPropertyName("Message_GUID")]
		public string MessageGuid { get; set; }
		[JsonPropertyName("email")]
		public string EmailAddress { get; set; }
		[JsonPropertyName("mj_campaign_id")]
		public long MailjetCampaignId { get; set; }
		[JsonPropertyName("mj_contact_id")]
		public long MailjetContactId { get; set; }
		[JsonPropertyName("customcampaign")]
		public string CustomCampaign { get; set; }
		[JsonPropertyName("CustomID")]
		public string CustomId { get; set; }
		[JsonPropertyName("Payload")]
		public string Payload { get; set; }
	}
}
