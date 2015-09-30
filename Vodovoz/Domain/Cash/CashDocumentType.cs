using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	public enum CashDocumentType
	{
		[Display (Name = "Приходный ордер")]
		Income,
		[Display (Name = "Расходный ордер")]
		Expense,
		[Display (Name = "Авансовый отчет")]
		AdvanceReport,
	}
}

