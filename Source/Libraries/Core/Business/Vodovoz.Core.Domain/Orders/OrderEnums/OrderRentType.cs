using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OrderRentType
	{
		[Display(Name = "Нет аренды")]
		None,

		[Display(Name = "Долгосрочная аренда")]
		NonFreeRent,

		[Display(Name = "Бесплатная аренда")]
		FreeRent,

		[Display(Name = "Посуточная аренда")]
		DailyRent
	}
}
