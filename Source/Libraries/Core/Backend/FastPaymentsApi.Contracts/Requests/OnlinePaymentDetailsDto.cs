using System.Text.Json.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Детали оплаты
	/// </summary>
	public class OnlinePaymentDetailsDto
	{
		/// <summary>
		/// Номер онлайн заказа
		/// </summary>
		[JsonPropertyName("id")]
		public int OnlineOrderId { get; set; }
		/// <summary>
		/// Данные по сумме оплаты
		/// </summary>
		[JsonPropertyName("amount")]
		public OnlinePaymentSumDetailsDto PaymentSumDetails { get; set; }
	}
}
