using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OnlineOrderStatus
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Обрабатывается")]
		Processing,
		[Display(Name = "Заказ оформлен")]
		OrderPerformed,
		[Display(Name = "Отменен")]
		Canceled
	}
}
