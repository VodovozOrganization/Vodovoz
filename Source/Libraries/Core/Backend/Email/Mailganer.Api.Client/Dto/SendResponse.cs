using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	public class SendResponse
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("message_id")]
		public string MessageId { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; }

		[JsonPropertyName("code")]
		public string Code { get; set; }
	}
}
