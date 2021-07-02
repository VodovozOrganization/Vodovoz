using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	public class FCMSendPushRequestDto
	{
		[JsonPropertyName("to")]
		public string To { get; set; }
		[JsonPropertyName("notification")]
		public FCMSendPushMessageDto Notification { get; set; }
	}
}
