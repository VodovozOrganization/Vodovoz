using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Payments
{
	public enum PaymentState
	{
		[Display(Name = "Нераспределен")]
		undistributed,
		[Display(Name = "Распределен")]
		distributed,
		[Display(Name = "Завершен")]
		completed,
		[Display(Name = "Отменен")]
		Cancelled
	}
}
