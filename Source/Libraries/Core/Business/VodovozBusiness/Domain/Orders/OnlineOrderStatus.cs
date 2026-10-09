using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Статус онлайн заказа
	/// </summary>
	public enum OnlineOrderStatus
	{
		/// <summary>
		/// Новый
		/// </summary>
		[Display(Name = "Новый")]
		New,
		/// <summary>
		/// Заказ оформлен(ы)
		/// </summary>
		[Display(Name = "Заказ(ы) оформлен(ы)")]
		OrderPerformed,
		/// <summary>
		/// Ожидает оплату
		/// </summary>
		[Display(Name = "Ожидает оплату")]
		WaitingForPayment,
		/// <summary>
		/// Отменен
		/// </summary>
		[Display(Name = "Отменен")]
		Canceled,
		//TODO разобраться с сортировкой
		/// <summary>
		/// Ожидает настройки автозаказа
		/// </summary>
		[Display(Name = "Ожидает настройки автозаказа")]
		PendingAutoOrderSettings
	}
}
