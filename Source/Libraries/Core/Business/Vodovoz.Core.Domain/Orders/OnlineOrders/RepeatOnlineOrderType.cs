using System.Text.Json.Serialization;

namespace Vodovoz.Core.Domain.Orders.OnlineOrders
{
	/// <summary>
	/// Частота повторения онлайн заказа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum RepeatOnlineOrderType
	{
		/// <summary>
		/// Раз в неделю
		/// </summary>
		OnePerWeek,
		/// <summary>
		/// Раз в две недели
		/// </summary>
		OneEveryTwoWeeks,
		/// <summary>
		/// Раз в три недели
		/// </summary>
		OneEveryThreeWeeks,
		/// <summary>
		/// Раз в четыре недели
		/// </summary>
		OneEveryFourWeeks
	}
}
