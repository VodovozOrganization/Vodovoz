using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Остатки ден.средств по компании на дату",
		Nominative = "Остаток ден.средств по компании на дату",
		Prepositional = "Остатке ден.средств по компании на дату",
		PrepositionalPlural = "Остатках ден.средств по компании на дату"
	)]
	[EntityPermission]
	public class CompanyBalanceByDay : PropertyChangedBase, IDomainObject, IBankStatementParsingResult
	{
		private decimal? _total;
		private DateTime _date;
		private IObservableList<FundsSummary> _funds = new ObservableList<FundsSummary>();

		public virtual int Id { get; set; }

		[Display(Name = "Дата выборки")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		[Display(Name = "Итоговый баланс на день")]
		public virtual decimal? Total
		{
			get => _total;
			set => SetField(ref _total, value);
		}

		[Display(Name = "Данные по формам денежных средств")]
		public virtual IObservableList<FundsSummary> FundsSummary
		{
			get => _funds;
			set => SetField(ref _funds, value);
		}

		public virtual string Name => "ИТОГО";
		public virtual string AccountNumber => string.Empty;
		public virtual string Bank => string.Empty;

		public static CompanyBalanceByDay Create(DateTime date) =>
			new CompanyBalanceByDay
			{
				Date = date
			};
	}
}
