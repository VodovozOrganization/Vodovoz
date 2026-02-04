using System;
using System.Text;
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
		public string Organization { get; set; }
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
				case BankAccountMovementDataType.TotalReceived:
					var sb = new StringBuilder();
					if(!Amount.HasValue)
					{
						return null;
					}
					
					const string duplicates = "Либо проверьте журнал платежей на дубли за этот период";
					
					if(StartDate == EndDate)
					{
						sb.Append(string.Format(messageTemplateWithSimpleDates, "платежам", StartDate.ToShortDateString()))
							.Append(". ")
							.Append(duplicates);
						
						return sb.ToString();
					}

					if(StartDate != EndDate)
					{
						sb.Append(string.Format(
								messageTemplateWithDifferentDates,
								"платежам",
								StartDate.ToShortDateString(),
								EndDate.ToShortDateString()))
							.Append(". ")
							.Append(duplicates);
						
						return sb.ToString();
					}
					break;
				case BankAccountMovementDataType.InitialBalance:
					if(!Amount.HasValue)
					{
						return $"Загрузите файл выписки за {StartDate.ToShortDateString()}";
					}
					
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
			string organization,
			BankAccountMovementDataType accountMovementDataType
			)
		{
			return new BankAccountsMovementsJournalNode
			{
				StartDate = startDate,
				EndDate = endDate,
				Bank = bank,
				Account = account,
				Organization = organization,
				AccountMovementDataType = accountMovementDataType
			};
		}
	}
}
