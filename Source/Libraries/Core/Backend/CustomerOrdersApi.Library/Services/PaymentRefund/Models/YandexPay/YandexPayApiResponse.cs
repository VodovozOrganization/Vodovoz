using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	/// <summary>
	/// Базовый ответ от API Яндекс Пэй 
	/// </summary>
	public class YandexPayApiResponse<T>
	{
		[JsonPropertyName("code")]
		public int? Code { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("data")]
		public T Data { get; set; }

		[JsonPropertyName("reason")]
		public string Reason { get; set; }

		[JsonPropertyName("reasonCode")]
		public string ReasonCode { get; set; }

		[JsonPropertyName("details")]
		public object Details { get; set; }
	}
}
