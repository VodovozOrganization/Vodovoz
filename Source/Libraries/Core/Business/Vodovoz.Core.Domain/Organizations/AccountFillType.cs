using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Organizations
{
	public enum AccountFillType
	{
		[Display(Name = "Банковская выписка")]
		BankStatement,
		[Display(Name = "Из Кассы ДВ")]
		CashSubdivision,
		[Display(Name = "Ручное")]
		Manual
	}
}
