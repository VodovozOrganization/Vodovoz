using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OrderRatingStatus
	{
		[Display(Name = "Новая")]
		New,
		[Display(Name = "Обработана")]
		Processed,
		[Display(Name = "Положительная")]
		Positive
	}
}
