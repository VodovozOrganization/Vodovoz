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
		OrderRescheduled,

		/// <summary>
		/// Менеджер открыл настройку автозаказа
		/// </summary>
		[Display(Name = "Менеджер открыл настройку автозаказа")]
		AutoOrderSettingsOpenedByManager,
		
		/// <summary>
		/// Менеджер открыл оплату
		/// </summary>
		[Display(Name = "Менеджер открыл оплату")]
		PaymentOpenedByManager,

		/// <summary>
		/// Первая неуспешная попытка списания по автозаказу
		/// </summary>
		[Display(Name = "Первая неуспешная попытка списания по автозаказу")]
		FirstUnsuccessfulChargeAttempt,

		/// <summary>
		/// Не удалось списать оплату по автозаказу
		/// </summary>
		[Display(Name = "Оплата автозаказа не прошла")]
		UnableProcessPaymentForAutoOrder,

		/// <summary>
		/// Менеджер обработал автозаказ
		/// </summary>
		[Display(Name = "Менеджер обработал автозаказ")]
		ManagerProcessedAutoOrder,

		/// <summary>
		/// Автозаказ подключен
		/// </summary>
		[Display(Name = "Автозаказ подключен")]
		AutoOrderEnabled,
		
		/// <summary>
		/// Заказ собран
		/// </summary>
		[Display(Name = "Заказ собран")]
		OrderCollected
	}
}
