using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class OrganizationJournalFactory : IOrganizationJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateOrganizationAutocompleteSelectorFactory()
		{
			return new SimpleEntitySelectorFactory<Organization, OrganizationDlg>(
				() =>
				{
					var organisationJournal =
						new SimpleEntityJournalViewModel<Organization, OrganizationDlg>(
							x => x.Name,
							() => new OrganizationDlg(),
							node => new OrganizationDlg(node.Id),
							UnitOfWorkFactory.GetDefaultFactory,
							ServicesConfig.CommonServices
						)
						{
							SelectionMode = JournalSelectionMode.Single
						};
					return organisationJournal;
				});
		}
		
		public IEntityAutocompleteSelectorFactory CreateOrganizationsForAvangardPaymentsAutocompleteSelectorFactory()
		{
			return new SimpleEntitySelectorFactory<Organization, OrganizationDlg>(
				() =>
				{
					var journal =
						new SimpleEntityJournalViewModel<Organization, OrganizationDlg>(
							x => x.Name,
							() => new OrganizationDlg(),
							node => new OrganizationDlg(node.Id),
							UnitOfWorkFactory.GetDefaultFactory,
							ServicesConfig.CommonServices
						)
						{
							SelectionMode = JournalSelectionMode.Single
						};
					journal.SetRestriction(Restrictions.Where<Organization>(x => x.AvangardShopId != null));
					return journal;
				});
		}
	}
}
