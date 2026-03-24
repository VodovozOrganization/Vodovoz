using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Тип события для уведомления клиента
	/// </summary>
	public enum CustomerNotificationEventType
	{
		// ЖДУ ОТВЕТ ОТ КОНСТАНТИНА (УДАЛЯТЬ ЛИ СТАРЫЕ?)

		/// <summary>
		/// Заказ оформляется
		/// </summary>
		[Display(Name = "Заказ оформляется")]
		OrderProcessing,
		
		/// <summary>
		/// Заказ оформлен
		/// </summary>
		[Display(Name = "Заказ оформлен")]
		OrderPerformed,
		
		/// <summary>
		/// Заказ отменён
		/// </summary>
		[Display(Name = "Заказ отменён")]
		OrderCanceled,
		
		/// <summary>
		/// Заказ доставляется
		/// </summary>
		[Display(Name = "Заказ доставляется")]
		OrderDelivering,

		//-----------------------------------------------------------
		// Ниже новые события (Оставить только их? Жду ответ Константина)

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
		OrderRescheduled,
	}
}
