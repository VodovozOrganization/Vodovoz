using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Статус позиции из корзины
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
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
