using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	/// <summary>
	/// Целевая корзина
	/// </summary>
	public class YandexPayTargetCart
	{
		/// <summary>
		/// Список товаров в корзине
		/// </summary>
		[JsonPropertyName("items")]
		public List<YandexPayCartItem> Items { get; set; }
	}
}
