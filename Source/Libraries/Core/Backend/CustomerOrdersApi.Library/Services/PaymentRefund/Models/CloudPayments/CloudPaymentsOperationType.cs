using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments
{
	/// <summary>
	/// Типы операций CloudPayments
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CloudPaymentsOperationType
	{
		/// <summary>
		/// Оплата
		/// </summary>
		Payment = 0,

		/// <summary>
		/// Возврат
		/// </summary>
		Refund = 1,

		/// <summary>
		/// Выплата на карту
		/// </summary>
		CardPayout = 2
	}
}
