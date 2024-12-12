using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Данные по расчетным счетам",
		Nominative = "Данные по расчетному счету",
		GenitivePlural = "Данные по расчетному счету")]
	public class BusinessAccountSummary : PropertyChangedBase, IDomainObject, IBankStatementParsingResult
	{
		private decimal? _total;
		private BusinessAccount _businessAccount;
		private BusinessActivitySummary _businessActivitySummary;
		
		public virtual int Id { get; set; }

		[Display(Name = "Баланс")]
		public virtual decimal? Total
		{
			get => _total;
			set => SetField(ref _total, value);
		}

		[Display(Name = "Расчетный счет")]
		public virtual BusinessAccount BusinessAccount
		{
			get => _businessAccount;
			set => SetField(ref _businessAccount, value);
		}

		[Display(Name = "Данные по направлению деятельности")]
		public virtual BusinessActivitySummary BusinessActivitySummary
		{
			get => _businessActivitySummary;
			set => SetField(ref _businessActivitySummary, value);
		}

		public virtual string Name => BusinessAccount?.Name;
		public virtual string Bank => BusinessAccount.Bank;
		public virtual string AccountNumber => BusinessAccount?.Number;
		
		public static BusinessAccountSummary Create(BusinessAccount businessAccount, BusinessActivitySummary businessActivitySummary)
			=> new BusinessAccountSummary
			{
				BusinessAccount = businessAccount,
				BusinessActivitySummary = businessActivitySummary
			};
	}
}
