using QS.Navigation;
using QS.Tdi;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Widgets.Organizations;

namespace Vodovoz.ViewModels.Factories
{
	public interface IOrganizationVersionsViewModelFactory
	{
		OrganizationVersionsViewModel CreateOrganizationVersionsViewModel(Organization organization, bool isEditable = true);
	}
}
