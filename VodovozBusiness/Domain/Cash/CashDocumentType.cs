using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	public enum CashDocumentType
	{
		[Display(Name = "Приходный ордер")]
		Income,
		[Display(Name = "Приходный ордер cамовывоз")]
		IncomeSelfDelivery,
		[Display(Name = "Расходный ордер")]
		Expense,
		[Display(Name = "Расходный ордер cамовывоз")]
		ExpenseSelfDelivery,
		[Display(Name = "Авансовый отчет")]
		AdvanceReport
	}
}

