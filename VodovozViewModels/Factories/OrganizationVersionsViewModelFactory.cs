using QS.Navigation;
using QS.Services;
using QS.Tdi;
using System;
using Vodovoz.Controllers;
using Vodovoz.Domain.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Widgets.Organizations;

namespace Vodovoz.ViewModels.Factories
{
	public class OrganizationVersionsViewModelFactory : IOrganizationVersionsViewModelFactory
	{
		private readonly ICommonServices _commonServices;

		public OrganizationVersionsViewModelFactory(ICommonServices commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
		}

		public OrganizationVersionsViewModel CreateOrganizationVersionsViewModel(Organization organization, IEmployeeJournalFactory employeeJournalFactory, INavigationManager navigationManager)
		{
			return new OrganizationVersionsViewModel(organization, _commonServices, new OrganizationVersionsController(organization), employeeJournalFactory, navigationManager/*parentTab*/);
		}
	}
}
