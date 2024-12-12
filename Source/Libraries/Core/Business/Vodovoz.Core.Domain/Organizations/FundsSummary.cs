using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Данные по формам денежных средств",
		Nominative = "Данные по формам денежных средств",
		GenitivePlural = "Данные по формам денежных средств")]
	public class FundsSummary : PropertyChangedBase, IDomainObject, IBankStatementParsingResult
	{
		private decimal? _total;
		private CompanyBalanceByDay _companyBalanceByDay;
		private Funds _funds;
		private IObservableList<BusinessActivitySummary> _businessActivitySummary = new ObservableList<BusinessActivitySummary>();

		public virtual int Id { get; set; }

		[Display(Name = "Баланс")]
		public virtual decimal? Total
		{
			get => _total;
			set => SetField(ref _total, value);
		}

		[Display(Name = "Итоговый баланс на день")]
		public virtual CompanyBalanceByDay CompanyBalanceByDay
		{
			get => _companyBalanceByDay;
			set => SetField(ref _companyBalanceByDay, value);
		}

		[Display(Name = "Форма денежных средств")]
		public virtual Funds Funds
		{
			get => _funds;
			set => SetField(ref _funds, value);
		}

		[Display(Name = "Данные по направлениям деятельности")]
		public virtual IObservableList<BusinessActivitySummary> BusinessActivitySummary
		{
			get => _businessActivitySummary;
			set => SetField(ref _businessActivitySummary, value);
		}

		public virtual string Name => Funds?.Name;
		public virtual string AccountNumber => string.Empty;
		public virtual string Bank => string.Empty;
		
		public static FundsSummary Create(Funds funds, CompanyBalanceByDay companyBalanceByDay)
			=> new FundsSummary
			{
				Funds = funds,
				CompanyBalanceByDay = companyBalanceByDay
			};
	}
}
