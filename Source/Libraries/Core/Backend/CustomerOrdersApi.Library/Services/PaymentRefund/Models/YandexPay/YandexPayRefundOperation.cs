using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	public class YandexPayRefundOperation
	{
		[JsonPropertyName("operationId")]
		public string OperationId { get; set; }

		[JsonPropertyName("status")]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public YandexPayOperationStatus Status { get; set; }

		[JsonPropertyName("externalOperationId")]
		public string ExternalOperationId { get; set; }
	}
}
