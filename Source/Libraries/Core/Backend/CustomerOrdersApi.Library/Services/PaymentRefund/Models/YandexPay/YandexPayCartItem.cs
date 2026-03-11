using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	public class YandexPayCartItem
	{
		[JsonPropertyName("productId")]
		public string ProductId { get; set; }

		[JsonPropertyName("quantity")]
		public YandexPayQuantity Quantity { get; set; }

		[JsonPropertyName("total")]
		public string Total { get; set; }
	}
}
