using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum RequestForCallStatus
	{
		[Display(Name = "Новая")]
		New,
		[Display(Name = "Заказ оформлен")]
		OrderPerformed,
		[Display(Name = "Закрыта")]
		Closed
	}
}
