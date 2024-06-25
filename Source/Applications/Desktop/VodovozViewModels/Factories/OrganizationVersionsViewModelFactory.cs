using QS.DomainModel.UoW;
using QS.Services;
using System;
using Vodovoz.Controllers;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.StoredResourceRepository;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Widgets.Organizations;

namespace Vodovoz.ViewModels.Factories
{
	public class OrganizationVersionsViewModelFactory : IOrganizationVersionsViewModelFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ICommonServices _commonServices;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IStoredResourceRepository _storedResourceRepository;

		public OrganizationVersionsViewModelFactory(IUnitOfWorkFactory uowFactory, ICommonServices commonServices, IEmployeeJournalFactory employeeJournalFactory, IStoredResourceRepository storedResourceRepository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_storedResourceRepository = storedResourceRepository ?? throw new ArgumentNullException(nameof(storedResourceRepository));
		}

		public OrganizationVersionsViewModel CreateOrganizationVersionsViewModel(Organization organization, bool isEditable = true)
		{
			return new OrganizationVersionsViewModel(organization, _commonServices, new OrganizationVersionsController(organization), _storedResourceRepository, _employeeJournalFactory, isEditable);
		}
	}
}
