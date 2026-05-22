using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V5.Orders.Templates
{
	/// <summary>
	/// Периодичность доставки из ИПЗ
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ExternalOnlineOrderDeliveryFrequency
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
