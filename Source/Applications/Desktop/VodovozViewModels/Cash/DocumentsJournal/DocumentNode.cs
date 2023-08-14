using QS.Project.Journal;
using System;
using Vodovoz.Domain.Cash;

namespace Vodovoz.ViewModels.Cash.DocumentsJournal
{
	public partial class DocumentsJournalViewModel
	{
		public class DocumentNode : JournalEntityNodeBase
		{
			public override string Title => Name;

			public string Name { get; set; }

			public string Description { get; set; }

			public CashDocumentType CashDocumentType { get; set; }

			public DateTime Date { get; set; }

			public string EmployeeSurname { get; set; }

			public string EmployeeName { get; set; }

			public string EmployeePatronymic { get; set; }

			public string CasherSurname { get; set; }

			public string CasherName { get; set; }

			public string CasherPatronymic { get; set; }

			public decimal Money { get; set; }

			public decimal MoneySigned
			{
				get
				{
					if(EntityType == typeof(Expense))
					{
						return -Money;
					}

					if(EntityType == typeof(AdvanceReport))
					{
						return 0;
					}

					return Money;
				}
			}

			public IncomeInvoiceDocumentType IncomeDocumentType { get; set; }

			public ExpenseInvoiceDocumentType ExpenseDocumentType { get; set; }

			public string Category { get; set; }
		}
	}
}
