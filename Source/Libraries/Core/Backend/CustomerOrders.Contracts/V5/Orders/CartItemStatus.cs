using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V5.Orders
{
	/// <summary>
	/// Статус позиции из корзины
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CartItemStatus
	{
		/// <summary>
		/// Доступен
		/// </summary>
		Active,
		/// <summary>
		/// Недоступен
		/// </summary>
		Blocked
	}
}
