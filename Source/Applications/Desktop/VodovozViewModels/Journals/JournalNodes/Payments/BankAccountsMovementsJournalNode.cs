using System;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Payments
{
	public class BankAccountsMovementsJournalNode
	{
		public int? Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Bank { get; set; }
		public string Account { get; set; }
		public BankAccountMovementDataType AccountMovementDataType { get; set; }
		public decimal? Amount { get; set; }
		public decimal? AmountFromProgram { get; set; }
		public decimal? Difference => Amount - AmountFromProgram;
		public bool HasDiscrepancy => !Amount.HasValue || (Difference.HasValue && Difference != 0);
		
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

		public static BankAccountsMovementsJournalNode NotLoaded(
			DateTime startDate,
			DateTime endDate,
			string bank,
			string account,
			BankAccountMovementDataType accountMovementDataType
			)
		{
			return new BankAccountsMovementsJournalNode
			{
				StartDate = startDate,
				EndDate = endDate,
				Bank = bank,
				Account = account,
				AccountMovementDataType = accountMovementDataType
			};
		}
	}
}
