using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Orders
{
	public abstract class PaymentTypeOrganizationSettings : PropertyChangedBase, IDomainObject
	{
		private Organization _organizationForOrder;
		
		public virtual int Id { get; set; }

		public virtual Organization OrganizationForOrder
		{
			get => _organizationForOrder;
			set => SetField(ref _organizationForOrder, value);
		}
		
		public abstract PaymentType PaymentType { get; }
	}
}
