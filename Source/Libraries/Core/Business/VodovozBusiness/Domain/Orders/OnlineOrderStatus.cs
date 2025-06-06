using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OnlineOrderStatus
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Заказ(ы) оформлен(ы)")]
		OrderPerformed,
		[Display(Name = "Отменен")]
		Canceled
	}
}
