using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.TempAdapters
{
	public class SubdivisionJournalFactory : ISubdivisionJournalFactory
	{
		private readonly SubdivisionFilterViewModel _filterViewModel;
		private IEmployeeJournalFactory _employeeJournalFactory;
		private ISalesPlanJournalFactory _salesPlanJournalFactory;
		private INomenclatureJournalFactory _nomenclatureSelectorFactory;

		public SubdivisionJournalFactory(SubdivisionFilterViewModel filterViewModel = null)
		{
			_filterViewModel = filterViewModel;
		}

		private void CreateNewDependencies()
		{
			_employeeJournalFactory = new EmployeeJournalFactory();
			_salesPlanJournalFactory = new SalesPlanJournalFactory();
			_nomenclatureSelectorFactory = new NomenclatureJournalFactory();
		}

		public IEntityAutocompleteSelectorFactory CreateSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory = null)
		{
			CreateNewDependencies();

			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				() => new SubdivisionsJournalViewModel(
					_filterViewModel ?? new SubdivisionFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					_employeeJournalFactory,
					_salesPlanJournalFactory,
					_nomenclatureSelectorFactory,
					Startup.AppDIContainer.BeginLifetimeScope()));
		}

		public IEntityAutocompleteSelectorFactory CreateDefaultSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory = null)
		{
			CreateNewDependencies();

			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				() => CreateSubdivisionsJournal(
					new SubdivisionFilterViewModel
					{
						SubdivisionType = SubdivisionType.Default
					},
					employeeSelectorFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateLogisticSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory = null)
		{
			CreateNewDependencies();

			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				() => CreateSubdivisionsJournal(
					new SubdivisionFilterViewModel
					{
						SubdivisionType = SubdivisionType.Logistic
					},
					employeeSelectorFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateCashSubdivisionAutocompleteSelectorFactory()
		{
			CreateNewDependencies();

			var selectorFactory = new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				CreateCashSubdivisionsJournal);
			return selectorFactory;
		}

		private SubdivisionsJournalViewModel CreateCashSubdivisionsJournal()
		{
			var filter = new SubdivisionFilterViewModel();
			filter.OnlyCashSubdivisions = true;

			return CreateSubdivisionsJournal(filter);
		}
		
		private SubdivisionsJournalViewModel CreateSubdivisionsJournal(
			SubdivisionFilterViewModel filter = null, IEntityAutocompleteSelectorFactory employeeSelectorFactory = null)
		{
			return new SubdivisionsJournalViewModel(
				filter ?? new SubdivisionFilterViewModel(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				_employeeJournalFactory,
				_salesPlanJournalFactory,
				_nomenclatureSelectorFactory,
				Startup.AppDIContainer.BeginLifetimeScope()
			);
		}
	}
}
