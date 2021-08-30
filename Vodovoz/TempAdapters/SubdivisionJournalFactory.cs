using QS.DomainModel.UoW;
using QS.Project.Journal.Actions.ViewModels;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.TempAdapters
{
	public class SubdivisionJournalFactory : ISubdivisionJournalFactory
	{
		private readonly SubdivisionFilterViewModel _filterViewModel;
		private IEmployeeJournalFactory _employeeJournalFactory;
		private ISalesPlanJournalFactory _salesPlanJournalFactory;
		private INomenclatureSelectorFactory _nomenclatureSelectorFactory;
		private EntitiesJournalActionsViewModel _entitiesJournalActionsViewModel;

		public SubdivisionJournalFactory(SubdivisionFilterViewModel filterViewModel = null)
		{
			_filterViewModel = filterViewModel;
		}
		
		private void CreateNewDependencies()
		{
			_employeeJournalFactory = new EmployeeJournalFactory();
			_salesPlanJournalFactory = new SalesPlanJournalFactory();
			_nomenclatureSelectorFactory = new NomenclatureSelectorFactory();
			_entitiesJournalActionsViewModel = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
		}
		
		private SubdivisionsJournalViewModel CreateNewInstanceOfSubdivisionsJournal(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory, SubdivisionFilterViewModel filterViewModel)
		{
			return new SubdivisionsJournalViewModel(
				_entitiesJournalActionsViewModel,
				filterViewModel ?? new SubdivisionFilterViewModel(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				employeeSelectorFactory ?? _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(),
				_salesPlanJournalFactory,
				_nomenclatureSelectorFactory);
		}
		
		public IEntityAutocompleteSelectorFactory CreateSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory = null)
		{
			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				() => CreateSubdivisionsJournal(employeeSelectorFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateDefaultSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory = null)
		{
			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				() => CreateDefaultSubdivisionsJournal(employeeSelectorFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateLogisticSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory = null)
		{
			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				() => CreateLogisticSubdivisionsJournal(employeeSelectorFactory));
		}
		
		public IEntitySelector CreateSubdivisionsSelector(IEntityAutocompleteSelectorFactory employeeSelectorFactory)
		{
			CreateNewDependencies();

			return CreateNewInstanceOfSubdivisionsJournal(employeeSelectorFactory, _filterViewModel);
		}
		
		public SubdivisionsJournalViewModel CreateSubdivisionsJournal(IEntityAutocompleteSelectorFactory employeeSelectorFactory)
		{
			CreateNewDependencies();

			return CreateNewInstanceOfSubdivisionsJournal(employeeSelectorFactory, _filterViewModel);
		}

		public SubdivisionsJournalViewModel CreateDefaultSubdivisionsJournal(IEntityAutocompleteSelectorFactory employeeSelectorFactory)
		{
			CreateNewDependencies();

			var filter = new SubdivisionFilterViewModel
			{
				SubdivisionType = SubdivisionType.Default
			};
			
			return CreateNewInstanceOfSubdivisionsJournal(employeeSelectorFactory, filter);
		}
		
		public SubdivisionsJournalViewModel CreateLogisticSubdivisionsJournal(IEntityAutocompleteSelectorFactory employeeSelectorFactory)
		{
			CreateNewDependencies();
			
			var filter = new SubdivisionFilterViewModel
			{
				SubdivisionType = SubdivisionType.Logistic
			};
			
			return CreateNewInstanceOfSubdivisionsJournal(employeeSelectorFactory, filter);
		}
	}
}
