using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Request
{
	public class CallsStatRequest
	{
		[JsonPropertyName("start_date")]
		public string StartDate { get; set; } = string.Empty;

		[JsonPropertyName("end_date")]
		public string EndDate { get; set; } = string.Empty;

		[JsonPropertyName("limit")]
		public string Limit { get; set; } = "1000";

		[JsonPropertyName("offset")]
		public string Offset { get; set; } = "0";
	}
}
