using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	public class YandexPayTargetCart
	{
		[JsonPropertyName("items")]
		public List<YandexPayCartItem> Items { get; set; }
	}
}
