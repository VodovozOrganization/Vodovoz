using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Contracts
{
	/// <summary>
	/// Статус онлайн заказа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ExternalOrderStatus
	{
		/// <summary>
		/// Оформляется
		/// </summary>
		[Display(Name = "Заказ оформляется")]
		OrderProcessing,
		/// <summary>
		/// Оформлен
		/// </summary>
		[Display(Name = "Заказ оформлен")]
		OrderPerformed,
		/// <summary>
		/// Доставляется
		/// </summary>
		[Display(Name = "Заказ доставляется")]
		OrderDelivering,
		/// <summary>
		/// Отменен
		/// </summary>
		[Display(Name = "Отменен")]
		Canceled
	}
}
