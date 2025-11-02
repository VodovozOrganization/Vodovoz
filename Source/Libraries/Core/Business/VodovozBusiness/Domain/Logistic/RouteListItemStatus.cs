using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public enum RouteListItemStatus
	{
		[Display(Name = "В пути")]
		EnRoute,
		[Display(Name = "Выполнен")]
		Completed,
		[Display(Name = "Доставка отменена")]
		Canceled,
		[Display(Name = "Недовоз")]
		Overdue,
		[Display(Name = "Передан")]
		Transfered
	}
}
