using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Статус начисления бонусов по заказу
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum OrderBonusesEarnStatus
	{
		Pending = 0,
		Earned = 1
	}
}
