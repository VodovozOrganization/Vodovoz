using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	/// <summary>
	/// Детали отмены возврата или платежа в ЮKassa
	/// </summary>
	public class YooKassaCancellationDetails
	{
		/// <summary>
		/// Инициатор отмены
		/// </summary>
		[JsonPropertyName("party")]
		public string Party { get; set; }

		/// <summary>
		/// Причина отмены
		/// </summary>
		[JsonPropertyName("reason")]
		public string Reason { get; set; }
	}
}
