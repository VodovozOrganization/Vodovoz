using System.Text.Json.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Данные по сумме оплаты
	/// </summary>
	public class OnlinePaymentSumDetailsDto
	{
		/// <summary>
		/// Сумма оплаты
		/// </summary>
		[JsonPropertyName("value")]
		public decimal PaymentSum { get; set; }
		/// <summary>
		/// Валюта
		/// </summary>
		[JsonPropertyName("currency")]
		public string Currency => CurrencyType.RUB.ToString();
	}

	/// <summary>
	/// Валюта
	/// </summary>
	public enum CurrencyType
	{
		/// <summary>
		/// Рубли
		/// </summary>
		RUB
	}
}
