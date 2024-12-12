using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.FastPayments
{
	public enum FastPaymentStatus
	{
		[Display(Name = "Обрабатывается")]
		Processing = 1,
		[Display(Name = "Отбракован")]
		Rejected,
		[Display(Name = "Исполнен")]
		Performed
	}
}
