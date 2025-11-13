using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum FiscalPaymentType
	{
		[Display(Name = "Безналичная")]
		Card,
		[Display(Name = "Наличная")]
		Cash,
		[Display(Name = "Предварительная (аванс)")]
		Prepaid,
		[Display(Name = "Постоплата (кредит)")]
		Postpay,
		[Display(Name = "Иная")]
		Other
	}
}
