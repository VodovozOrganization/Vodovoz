using System.Text.Json.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Информация по оплате
	/// </summary>
	public class FastPaymentStatusDto
	{
		/// <summary>
		/// Статус
		/// </summary>
		[JsonPropertyName("status")]
		public RequestPaymentStatus PaymentStatus { get; set; }
		/// <summary>
		/// Детали
		/// </summary>
		[JsonPropertyName("details")]
		public OnlinePaymentDetailsDto PaymentDetails { get; set; }
	}
}
