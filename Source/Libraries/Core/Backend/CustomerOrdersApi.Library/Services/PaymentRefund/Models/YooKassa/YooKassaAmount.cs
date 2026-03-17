using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	/// <summary>
	/// Модель суммы и валюты в формате ЮKassa
	/// </summary>
	public class YooKassaAmount
	{
		/// <summary>
		/// Сумма
		/// </summary>
		[JsonPropertyName("value")]
		public string Value { get; set; }

		/// <summary>
		/// Код валюты в формате ISO-4217
		/// </summary>
		[JsonPropertyName("currency")]
		public string Currency { get; set; } = "RUB";
	}
}
