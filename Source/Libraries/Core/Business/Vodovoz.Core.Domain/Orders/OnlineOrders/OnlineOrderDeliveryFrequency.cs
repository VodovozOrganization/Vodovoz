using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.Core.Domain.Orders.OnlineOrders
{
	/// <summary>
	/// Периодичность доставки онлайн заказа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum OnlineOrderDeliveryFrequency
	{
		/// <summary>
		/// Раз в неделю
		/// </summary>
		[Display(Name = "Раз в неделю")]
		OnePerWeek,
		/// <summary>
		/// Раз в две недели
		/// </summary>
		[Display(Name = "Раз в две недели")]
		OneEveryTwoWeeks,
		/// <summary>
		/// Раз в три недели
		/// </summary>
		[Display(Name = "Раз в три недели")]
		OneEveryThreeWeeks,
		/// <summary>
		/// Раз в четыре недели
		/// </summary>
		[Display(Name = "Раз в четыре недели")]
		OneEveryFourWeeks
	}
}
