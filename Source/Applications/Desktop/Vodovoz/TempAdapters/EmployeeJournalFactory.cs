using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Core.DataService;
using QS.Navigation;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalSelectors;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class EmployeeJournalFactory : IEmployeeJournalFactory
	{
		private readonly INavigationManager _navigationManager;
		private EmployeeFilterViewModel _employeeJournalFilter;
		private IAuthorizationServiceFactory _authorizationServiceFactory;
		private IEmployeeWageParametersFactory _employeeWageParametersFactory;
		private IEmployeeJournalFactory _employeeJournalFactory;
		private ISubdivisionJournalFactory _subdivisionJournalFactory;
		private IEmployeePostsJournalFactory _employeePostsJournalFactory;
		private ICashDistributionCommonOrganisationProvider _cashDistributionCommonOrganisationProvider;
		private ISubdivisionParametersProvider _subdivisionParametersProvider;
		private IWageCalculationRepository _wageCalculationRepository;
		private IEmployeeRepository _employeeRepository;
		private IValidationContextFactory _validationContextFactory;
		private IPhonesViewModelFactory _phonesViewModelFactory;
		private IWarehouseRepository _warehouseRepository;
		private IRouteListRepository _routeListRepository;
		private IAttachmentsViewModelFactory _attachmentsViewModelFactory;

		public EmployeeJournalFactory(EmployeeFilterViewModel employeeJournalFilter = null)
		{
			_employeeJournalFilter = employeeJournalFilter;

			_authorizationServiceFactory = new AuthorizationServiceFactory();
			_employeeWageParametersFactory = new EmployeeWageParametersFactory();
			_employeeJournalFactory = this;
			_subdivisionJournalFactory = new SubdivisionJournalFactory();
			_employeePostsJournalFactory = new EmployeePostsJournalFactory();
			_validationContextFactory = new ValidationContextFactory();
			_phonesViewModelFactory = new PhonesViewModelFactory(new PhoneRepository());
			_attachmentsViewModelFactory = new AttachmentsViewModelFactory();
			_navigationManager = MainClass.MainWin.NavigationManager;
		}

		private void CreateNewDependencies()
		{
			_cashDistributionCommonOrganisationProvider =
				new CashDistributionCommonOrganisationProvider(new OrganizationParametersProvider(new ParametersProvider()));

			_subdivisionParametersProvider = new SubdivisionParametersProvider(new ParametersProvider());
			_wageCalculationRepository = new WageCalculationRepository();
			_employeeRepository = new EmployeeRepository();
			_warehouseRepository = new WarehouseRepository();
			_routeListRepository = new RouteListRepository(new StockRepository(), new BaseParametersProvider(new ParametersProvider()));
		}

		public EmployeesJournalViewModel CreateEmployeesJournal(EmployeeFilterViewModel filterViewModel = null)
		{
			CreateNewDependencies();

			return new EmployeesJournalViewModel(
				filterViewModel ?? _employeeJournalFilter ?? new EmployeeFilterViewModel(),
				_authorizationServiceFactory,
				ServicesConfig.CommonServices,
				UnitOfWorkFactory.GetDefaultFactory,
				MainClass.AppDIContainer
			);
		}

		public void SetEmployeeFilterViewModel(EmployeeFilterViewModel filter)
		{
			_employeeJournalFilter = filter;
		}
		
		public IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory()
		{
			CreateNewDependencies();
			
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() => CreateEmployeesJournal()
			);
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingDriverEmployeeAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				CreateWorkingDriverEmployeeJournal
			);
		}
		
		public EmployeesJournalViewModel CreateWorkingDriverEmployeeJournal()
		{
			CreateNewDependencies();
			
			var driverFilter = new EmployeeFilterViewModel
			{
				HidenByDefault = true,
			};

			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.Category = EmployeeCategory.driver);

			return CreateEmployeesJournal(driverFilter);
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingOfficeEmployeeAutocompleteSelectorFactory()
		{
			CreateNewDependencies();
			
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() =>
				{
					var officeFilter = new EmployeeFilterViewModel
					{
						HidenByDefault = true,
					};

					officeFilter.SetAndRefilterAtOnce(
						x => x.Status = EmployeeStatus.IsWorking,
						x => x.Category = EmployeeCategory.office);
					
					return CreateEmployeesJournal(officeFilter);
				}
			);
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingEmployeeAutocompleteSelectorFactory()
		{
			CreateNewDependencies();

			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() =>
				{
					var filter = new EmployeeFilterViewModel
					{
						HidenByDefault = true,
						Status = EmployeeStatus.IsWorking
					};

					return new EmployeesJournalViewModel(
						filter,
						_authorizationServiceFactory,
						ServicesConfig.CommonServices,
						UnitOfWorkFactory.GetDefaultFactory,
						MainClass.AppDIContainer
					);
				}
			);
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingForwarderEmployeeAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				CreateWorkingForwarderEmployeeJournal
			);
		}

		public EmployeesJournalViewModel CreateWorkingForwarderEmployeeJournal()
		{
			CreateNewDependencies();
			
			var forwarderFilter = new EmployeeFilterViewModel
			{
				HidenByDefault = true,
			};
					
			forwarderFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.Category = EmployeeCategory.forwarder);
					
			return CreateEmployeesJournal(forwarderFilter);
		}
	}
}
