using System;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Payments
{
	public class BankAccountsMovementsJournalNode
	{
		public int Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Bank { get; set; }
		public string Account { get; set; }
		public string Name { get; set; }
		public BankAccountMovementDataType AccountMovementDataType { get; set; }
		public decimal Amount { get; set; }
		public decimal? AmountFromProgram { get; set; }
		public decimal? Difference => Amount - AmountFromProgram;
		public bool HasDiscrepancy => Difference.HasValue && Difference != 0;
		
		public string GetDiscrepancyDescription()
		{
			const string messageTemplateWithSimpleDates = "Есть расхождения по {0}. Перезагрузите выписку за {1}";
			const string messageTemplateWithDifferentDates = "Есть расхождения по {0}. Перезагрузите выписку c {1} по {2}";
			
			if(!HasDiscrepancy)
			{
				return null;
			}

			switch(AccountMovementDataType)
			{
				case BankAccountMovementDataType.InitialBalance:
					if(StartDate == EndDate)
					{
						return string.Format(messageTemplateWithSimpleDates, "платежам", StartDate.ToShortDateString());
					}

					if(StartDate != EndDate)
					{
						return string.Format(
							messageTemplateWithDifferentDates,
							"платежам",
							StartDate.ToShortDateString(),
							EndDate.ToShortDateString());
					}
					break;
				case BankAccountMovementDataType.TotalReceived:
					if(StartDate == EndDate)
					{
						return string.Format(messageTemplateWithSimpleDates, "остатку", StartDate.ToShortDateString());
					}

					if(StartDate != EndDate)
					{
						return string.Format(
							messageTemplateWithDifferentDates,
							"остатку",
							StartDate.ToShortDateString(),
							EndDate.ToShortDateString());
					}
					break;
			}
			
			return null;
		}
	}
}
