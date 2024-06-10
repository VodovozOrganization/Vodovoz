using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	public enum OnlineOrderPaymentStatus
	{
		[Display(Name = "Не оплачен")]
		UnPaid,
		[Display(Name = "Оплачен")]
		Paid
	}
}
