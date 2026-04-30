using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts
{
	/// <summary>
	/// Формы оплат онлайн заказа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ExternalOrderPaymentType
	{
		/// <summary>
		/// Наличная
		/// </summary>
		Cash,
		/// <summary>
		/// Терминал
		/// </summary>
		Terminal,
		/// <summary>
		/// Оплачено онлайн
		/// </summary>
		PaidOnline
	}
}
