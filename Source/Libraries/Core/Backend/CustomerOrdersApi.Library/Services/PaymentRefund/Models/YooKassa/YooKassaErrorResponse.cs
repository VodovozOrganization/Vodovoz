using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	/// <summary>
	/// Модель ошибки API ЮKassa
	/// </summary>
	public class YooKassaErrorResponse
	{
		[JsonPropertyName("type")]
		public string Type { get; set; }

		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("code")]
		public string Code { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("parameter")]
		public string Parameter { get; set; }

		[JsonPropertyName("retry_after")]
		public int? RetryAfter { get; set; }
	}
}
