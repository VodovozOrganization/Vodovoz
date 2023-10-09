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
		private readonly ComplaintKindJournalFilterViewModel _filterViewModel;
		private IEmployeeJournalFactory _employeeJournalFactory;
		private ISalesPlanJournalFactory _salesPlanJournalFactory;
		private INomenclatureJournalFactory _nomenclatureSelectorFactory;

		public ComplaintKindJournalFactory(INavigationManager navigationManager , ComplaintKindJournalFilterViewModel filterViewModel = null)
		{
			_navigationManager = navigationManager ?? throw new System.ArgumentNullException(nameof(navigationManager));
			_filterViewModel = filterViewModel;
		}

		private void CreateNewDependencies()
		{
			_employeeJournalFactory = new EmployeeJournalFactory(_navigationManager);
			_salesPlanJournalFactory = new SalesPlanJournalFactory();
			_nomenclatureSelectorFactory = new NomenclatureJournalFactory();
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
