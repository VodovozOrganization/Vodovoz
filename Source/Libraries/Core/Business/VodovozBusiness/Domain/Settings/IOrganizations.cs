using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Settings
{
	public interface IOrganizations
	{
		IObservableList<Organization> Organizations { get; }
	}
}
