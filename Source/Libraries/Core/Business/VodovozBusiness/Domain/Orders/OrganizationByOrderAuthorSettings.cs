using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Vodovoz;

namespace VodovozBusiness.Domain.Orders
{
	public class OrganizationByOrderAuthorSettings : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual OrganizationBasedOrderContentSettings OrganizationBasedOrderContentSettings { get; set; }
		public virtual IObservableList<Subdivision> OrderAuthorsSubdivisions { get; set; } = new ObservableList<Subdivision>();
	}
}
