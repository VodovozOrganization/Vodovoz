using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Complaints;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.TempAdapters
{
	public class ComplaintKindJournalFactory : IComplaintKindJournalFactory
	{
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ComplaintKindJournalFilterViewModel _filterViewModel;
		private IEmployeeJournalFactory _employeeJournalFactory;
		private ISalesPlanJournalFactory _salesPlanJournalFactory;
		private INomenclatureJournalFactory _nomenclatureSelectorFactory;

		public ComplaintKindJournalFactory(INavigationManager navigationManager, ILifetimeScope lifetimeScope, ComplaintKindJournalFilterViewModel filterViewModel = null)
		{
			_navigationManager = navigationManager ?? throw new System.ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new System.ArgumentNullException(nameof(lifetimeScope));
			_filterViewModel = filterViewModel;
		}

		private void CreateNewDependencies()
		{
			_employeeJournalFactory = new EmployeeJournalFactory(_navigationManager);
			_salesPlanJournalFactory = new SalesPlanJournalFactory();
			_nomenclatureSelectorFactory = new NomenclatureJournalFactory(_lifetimeScope);
		}

		public IEntityAutocompleteSelectorFactory CreateComplaintKindAutocompleteSelectorFactory()
		{
			CreateNewDependencies();

			return new EntityAutocompleteSelectorFactory<ComplaintKindJournalViewModel>(typeof(ComplaintKind),
				() =>
				{
					var journalViewModel = new ComplaintKindJournalViewModel(
						_filterViewModel ?? new ComplaintKindJournalFilterViewModel(), 
						UnitOfWorkFactory.GetDefaultFactory, 
						ServicesConfig.CommonServices,
						_navigationManager,
						_employeeJournalFactory, 
						_salesPlanJournalFactory, 
						_nomenclatureSelectorFactory, 
						Startup.AppDIContainer.BeginLifetimeScope()
						);

					return journalViewModel;
				});
		}
	}
}
