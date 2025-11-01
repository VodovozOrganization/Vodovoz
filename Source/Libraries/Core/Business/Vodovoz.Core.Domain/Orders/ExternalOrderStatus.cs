using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.Core.Domain.Orders
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
		/// Ожидание оплаты
		/// </summary>
		[Display(Name = "Ожидание оплаты")]
		WaitingForPayment,
		/// <summary>
		/// Собирается
		/// </summary>
		[Display(Name = "Заказ собирается")]
		OrderCollecting,
		/// <summary>
		/// Доставляется
		/// </summary>
		[Display(Name = "Заказ доставляется")]
		OrderDelivering,
		/// <summary>
		/// Выполнен
		/// </summary>
		[Display(Name = "Заказ выполнен")]
		OrderCompleted,
		/// <summary>
		/// Отменен
		/// </summary>
		[Display(Name = "Отменен")]
		Canceled
	}
}
