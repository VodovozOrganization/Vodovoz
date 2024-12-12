using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OrderPaymentStatus
	{
		[Display(Name = "Нет")]
		None,

		[Display(Name = "Оплачен")]
		Paid,

		[Display(Name = "Частично оплачен")]
		PartiallyPaid,

		[Display(Name = "Не оплачен")]
		UnPaid
	}
}
