using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public enum RouteListStatus
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Подтвержден")]
		Confirmed,
		[Display(Name = "На погрузке")]
		InLoading,
		[Display(Name = "В пути")]
		EnRoute,
		[Display(Name = "Доставлен")]
		Delivered,
		[Display(Name = "Сдаётся")]
		OnClosing,
		[Display(Name = "Проверка километража")]
		MileageCheck,
		[Display(Name = "Закрыт")]
		Closed
	}
}
