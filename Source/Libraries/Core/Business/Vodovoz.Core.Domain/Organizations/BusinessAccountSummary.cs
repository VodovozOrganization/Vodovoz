using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Organizations
{
	public class BusinessAccountSummary : PropertyChangedBase, IDomainObject
	{
		private decimal _total;
		private BusinessAccount _businessAccount;
		private BusinessActivitySummary _businessActivitySummary;
		
		public virtual int Id { get; set; }

		public virtual decimal Total
		{
			get => _total;
			set => SetField(ref _total, value);
		}
		
		public virtual BusinessAccount BusinessAccount
		{
			get => _businessAccount;
			set => SetField(ref _businessAccount, value);
		}
		
		public virtual BusinessActivitySummary BusinessActivitySummary
		{
			get => _businessActivitySummary;
			set => SetField(ref _businessActivitySummary, value);
		}

		public static BusinessAccountSummary Create(BusinessAccount businessAccount, BusinessActivitySummary businessActivitySummary)
			=> new BusinessAccountSummary
			{
				BusinessAccount = businessAccount,
				BusinessActivitySummary = businessActivitySummary
			};
	}
}
