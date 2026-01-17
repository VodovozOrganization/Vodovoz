using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	public enum Reason
	{
		[Display(Name = "Неизвестна")] Unknown,
		[Display(Name = "Сервис")] Service,
		[Display(Name = "Аренда")] Rent,
		[Display(Name = "Расторжение")] Cancellation,
		[Display(Name = "Продажа")] Sale
	}
}
