using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MailganerEventsDistributorApi.DTO
{
	public class EmailEvent
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("messages")]
		public ICollection<EmailEventMessage> Messages { get; set; }
	}
}
