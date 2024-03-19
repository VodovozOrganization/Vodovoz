using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	public enum OnlineOrderPaymentType
	{
		[Display(Name = "Наличная")]
		Cash,
		[Display(Name = "Терминал")]
		Terminal,
		[Display(Name = "Оплачено онлайн")]
		PaidOnline
	}
}
