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
		[Display(Name = "Курьер в пути к клиенту")]
		CourierOnTheWay,

		/// <summary>
		/// Курьер задерживается
		/// </summary>
		[Display(Name = "Курьер задерживается")]
		CourierIsLate,

		/// <summary>
		/// Доставка выполнена
		/// </summary>
		[Display(Name = "Доставка выполнена")]
		DeliveryCompleted,

		/// <summary>
		/// Заказ оплачен
		/// </summary>
		[Display(Name = "Заказ оплачен")]
		OrderPaid,
		
		/// <summary>
		/// Заказ ожидает оплаты
		/// </summary>
		[Display(Name = "Заказ ожидает оплаты")]
		OrderAwaitingPayment,
		
		/// <summary>
		/// Заказ перенесен на другое время
		/// </summary>
		[Display(Name = "Заказ перенесен на другое время")]
		OrderRescheduled
	}
}
