using QS.Services;
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
		private readonly IEmployeeJournalFactory _employeeJournalFactory;

		public OrganizationVersionsViewModelFactory(ICommonServices commonServices, IEmployeeJournalFactory employeeJournalFactory)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)); ;
		}

		public OrganizationVersionsViewModel CreateOrganizationVersionsViewModel(Organization organization)
		{
			return new OrganizationVersionsViewModel(organization, _commonServices, new OrganizationVersionsController(organization), _employeeJournalFactory);
		}
	}
}
