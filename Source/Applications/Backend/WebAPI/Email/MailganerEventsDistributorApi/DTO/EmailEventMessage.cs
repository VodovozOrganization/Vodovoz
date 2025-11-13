using System;
using System.Text.Json.Serialization;

namespace MailganerEventsDistributorApi.DTO
{

	public class EmailEventMessage
	{
		[JsonPropertyName("message_id")]
		public string MessageId { get; set; }

		[JsonPropertyName("x_track_id")]
		public string TrackId { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("reason")]
		public string Reason { get; set; }

		[JsonPropertyName("created_at")]
		public long Timestamp { get; set; }
	}
}
