using System;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Core.Domain.Organizations
{
	public class CompanyBalanceByDay : PropertyChangedBase, IDomainObject
	{
		private DateTime _date;
		private IObservableList<FundsSummary> _funds = new ObservableList<FundsSummary>();
		
		public virtual int Id { get; set; }

		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}
		
		public virtual IObservableList<FundsSummary> FundsSummary
		{
			get => _funds;
			set => SetField(ref _funds, value);
		}

		public static CompanyBalanceByDay Create(DateTime date) =>
			new CompanyBalanceByDay
			{
				Date = date
			};
	}
}
