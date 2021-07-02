using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	public class FCMSendPushMessageDto
	{
		[JsonPropertyName("title")]
		public string Title { get; set; }
		[JsonPropertyName("body")]
		public string Body { get; set; }
	}
}
