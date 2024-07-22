using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Organizations
{
	public enum AccountFillType
	{
		[Display(Name = "Банковская выписка")]
		BankStatement,
		[Display(Name = "Из Кассы БЦ")]
		CashSubdivisionBC,
		[Display(Name = "Из Кассы БЦ София")]
		CashSubdivisionBCSofiya,
		[Display(Name = "Ручное")]
		Manual
	}
}
