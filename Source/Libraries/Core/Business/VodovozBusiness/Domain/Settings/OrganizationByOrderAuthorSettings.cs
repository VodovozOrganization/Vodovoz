using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Vodovoz;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Настройки для подбора организации по авторам заказа
	/// </summary>
	public class OrganizationByOrderAuthorSettings : PropertyChangedBase, IDomainObject
	{
		public static short DefaultSetForAuthorNotIncludedSet = 1;
		public virtual int Id { get; set; }
		public virtual OrganizationBasedOrderContentSettings OrganizationBasedOrderContentSettings { get; set; }
		public virtual IObservableList<Subdivision> OrderAuthorsSubdivisions { get; set; } = new ObservableList<Subdivision>();
	}
}
