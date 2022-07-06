using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "место, откуда проведены оплаты",
		Nominative = "место, откуда проведена оплата")]
	[HistoryTrace]
	[EntityPermission]
	public class PaymentFrom : PropertyChangedBase, IDomainObject
	{
		private Organization _organizationForAvangardPayments;
		
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }

		public virtual Organization OrganizationForAvangardPayments
		{
			get => _organizationForAvangardPayments;
			set => SetField(ref _organizationForAvangardPayments, value);
		}
	}
}
