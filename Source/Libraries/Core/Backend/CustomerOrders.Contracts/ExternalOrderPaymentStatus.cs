using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts
{
	/// <summary>
	/// Статус оплаты заказа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ExternalOrderPaymentStatus
	{
		/// <summary>
		/// Неоплачен
		/// </summary>
		UnPaid,
		/// <summary>
		/// Оплачен
		/// </summary>
		Paid
	}
}
