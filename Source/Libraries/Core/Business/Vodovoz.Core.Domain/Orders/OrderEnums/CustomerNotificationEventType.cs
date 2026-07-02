using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Тип события для уведомления клиента
	/// </summary>
	public enum CustomerNotificationEventType
	{
		/// <summary>
		/// Курьер назначен
		/// </summary>
		[Display(Name = "Курьер назначен")]
		CourierAssigned,
		
		/// <summary>
		/// Курьер в пути к клиенту
		/// </summary>
		[Display(Name = "Курьер уже в пути")]
		CourierOnTheWay,

		/// <summary>
		/// Курьер задерживается
		/// </summary>
		[Display(Name = "Курьер задерживается")]
		CourierIsLate,

		/// <summary>
		/// Доставка выполнена
		/// </summary>
		[Display(Name = "Заказ доставлен")]
		DeliveryCompleted,

		/// <summary>
		/// Заказ оплачен
		/// </summary>
		[Display(Name = "Оплата прошла успешно")]
		OrderPaid,
		
		/// <summary>
		/// Заказ ожидает оплаты
		/// </summary>
		[Display(Name = "Ожидается оплата")]
		OrderAwaitingPayment,
		
		/// <summary>
		/// Заказ перенесен на другое время
		/// </summary>
		[Display(Name = "Доставка перенесена")]
		OrderRescheduled
	}
}
