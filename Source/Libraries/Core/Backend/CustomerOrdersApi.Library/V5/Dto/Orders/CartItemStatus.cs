using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
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
