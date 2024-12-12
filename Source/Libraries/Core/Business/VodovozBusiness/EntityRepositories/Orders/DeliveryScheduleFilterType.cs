using System.ComponentModel.DataAnnotations;

namespace Vodovoz.EntityRepositories.Orders
{
	public enum DeliveryScheduleFilterType
	{
		[Display(Name = "Начало доставки")]
		DeliveryStart = 0,
		[Display(Name = "Окончание доставки")]
		DeliveryEnd = 1,
		[Display(Name = "Строгое попадание")]
		DeliveryStartAndEnd = 2,
		[Display(Name = "Время создания заказа")]
		OrderCreateDate = 3
	}
}
