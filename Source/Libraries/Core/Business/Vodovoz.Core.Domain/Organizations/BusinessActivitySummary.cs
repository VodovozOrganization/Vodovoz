using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Core.Domain.Organizations
{
	public class BusinessActivitySummary : PropertyChangedBase, IDomainObject
	{
		private decimal _total;
		private BusinessActivity _businessActivity;
		private FundsSummary _fundsSummary;
		private IObservableList<BusinessAccountSummary> _businessAccountsSummary = new ObservableList<BusinessAccountSummary>();
		
		public virtual int Id { get; set; }

		public virtual decimal Total
		{
			get => _total;
			set => SetField(ref _total, value);
		}
		
		public virtual BusinessActivity BusinessActivity
		{
			get => _businessActivity;
			set => SetField(ref _businessActivity, value);
		}
		
		public virtual FundsSummary FundsSummary
		{
			get => _fundsSummary;
			set => SetField(ref _fundsSummary, value);
		}
		
		public virtual IObservableList<BusinessAccountSummary> BusinessAccountsSummary
		{
			get => _businessAccountsSummary;
			set => SetField(ref _businessAccountsSummary, value);
		}

		public static BusinessActivitySummary Create(BusinessActivity businessActivity, FundsSummary fundsSummary)
			=> new BusinessActivitySummary
			{
				BusinessActivity = businessActivity,
				FundsSummary = fundsSummary
			};
	}
}
