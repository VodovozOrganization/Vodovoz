using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	/// <summary>
	/// Запрос на создание возврата в API ЮKassa
	/// </summary>
	public class YooKassaRefundRequest
	{
		/// <summary>
		/// Сумма возврата
		/// </summary>
		[JsonPropertyName("amount")]
		public YooKassaAmount Amount { get; set; }

		/// <summary>
		/// Идентификатор платежа, по которому совершается возврат
		/// </summary>
		[JsonPropertyName("payment_id")]
		public string PaymentId { get; set; }

		/// <summary>
		/// Описание возврата
		/// </summary>
		[JsonPropertyName("description")]
		public string Description { get; set; }

		/// <summary>
		/// Метаданные возврата
		/// </summary>
		[JsonPropertyName("metadata")]
		public Dictionary<string, string> Metadata { get; set; }
	}
}
