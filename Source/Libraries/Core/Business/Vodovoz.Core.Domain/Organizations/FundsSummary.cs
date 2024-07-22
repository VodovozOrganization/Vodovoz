using System;
using NHibernate.Engine.Query;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Core.Domain.Organizations
{
	public class FundsSummary : PropertyChangedBase, IDomainObject
	{
		private decimal _total;
		private Funds _funds;
		private IObservableList<BusinessActivitySummary> _businessActivitySummary = new ObservableList<BusinessActivitySummary>();
		
		public virtual int Id { get; set; }

		public virtual decimal Total
		{
			get => _total;
			set => SetField(ref _total, value);
		}
		
		public virtual Funds Funds
		{
			get => _funds;
			set => SetField(ref _funds, value);
		}
		
		public virtual IObservableList<BusinessActivitySummary> BusinessActivitySummary
		{
			get => _businessActivitySummary;
			set => SetField(ref _businessActivitySummary, value);
		}
		
		public static FundsSummary Create(Funds funds)
			=> new FundsSummary
			{
				Funds = funds
			};
	}
}
