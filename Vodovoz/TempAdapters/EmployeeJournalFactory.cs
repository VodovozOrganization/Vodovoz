using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Service.BaseParametersServices;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
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
		private readonly EmployeeFilterViewModel _employeeJournalFilter;
		private IAuthorizationServiceFactory _authorizationServiceFactory;
		private IEmployeeWageParametersFactory _employeeWageParametersFactory;
		private IEmployeeJournalFactory _employeeJournalFactory;
		private ISubdivisionJournalFactory _subdivisionJournalFactory;
		private IEmployeePostsJournalFactory _employeePostsJournalFactory;
		private ICashDistributionCommonOrganisationProvider _cashDistributionCommonOrganisationProvider;
		private ISubdivisionService _subdivisionService;
		private IEmailServiceSettingAdapter _emailServiceSettingAdapter;
		private IWageCalculationRepository _wageCalculationRepository;
		private IEmployeeRepository _employeeRepository;
		private IValidationContextFactory _validationContextFactory;
		private IPhonesViewModelFactory _phonesViewModelFactory;
		
		public EmployeeJournalFactory(EmployeeFilterViewModel employeeJournalFilter = null)
		{
			_employeeJournalFilter = employeeJournalFilter;
		}

		private void CreateNewDependencies()
		{
			_authorizationServiceFactory = new AuthorizationServiceFactory();
			_employeeWageParametersFactory = new EmployeeWageParametersFactory();
			_employeeJournalFactory = new EmployeeJournalFactory();
			_subdivisionJournalFactory = new SubdivisionJournalFactory();
			_employeePostsJournalFactory = new EmployeePostsJournalFactory();
        
			_cashDistributionCommonOrganisationProvider =
				new CashDistributionCommonOrganisationProvider(new OrganizationParametersProvider(new ParametersProvider()));
        
			_subdivisionService = SubdivisionParametersProvider.Instance;
			_emailServiceSettingAdapter = new EmailServiceSettingAdapter();
			_wageCalculationRepository = WageSingletonRepository.GetInstance();
			_employeeRepository = EmployeeSingletonRepository.GetInstance();
			_validationContextFactory = new ValidationContextFactory();
			_phonesViewModelFactory = new PhonesViewModelFactory(new PhoneRepository());
		}
		
		public IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory()
		{
			CreateNewDependencies();
			
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() => new EmployeesJournalViewModel(
					_employeeJournalFilter ?? new EmployeeFilterViewModel(),
					_authorizationServiceFactory,
					_employeeWageParametersFactory,
					_employeeJournalFactory,
					_subdivisionJournalFactory,
					_employeePostsJournalFactory,
					_cashDistributionCommonOrganisationProvider,
					_subdivisionService,
					_emailServiceSettingAdapter,
					_wageCalculationRepository,
					_employeeRepository,
					_validationContextFactory,
					_phonesViewModelFactory,
					ServicesConfig.CommonServices,
					UnitOfWorkFactory.GetDefaultFactory));
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
				Status = EmployeeStatus.IsWorking,
				Category = EmployeeCategory.driver
			};
					
			return new EmployeesJournalViewModel(
				driverFilter,
				_authorizationServiceFactory,
				_employeeWageParametersFactory,
				_employeeJournalFactory,
				_subdivisionJournalFactory,
				_employeePostsJournalFactory,
				_cashDistributionCommonOrganisationProvider,
				_subdivisionService,
				_emailServiceSettingAdapter,
				_wageCalculationRepository,
				_employeeRepository,
				_validationContextFactory,
				_phonesViewModelFactory,
				ServicesConfig.CommonServices,
				UnitOfWorkFactory.GetDefaultFactory);
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
						Status = EmployeeStatus.IsWorking,
						Category = EmployeeCategory.office
					};
					
					return new EmployeesJournalViewModel(
						officeFilter,
						_authorizationServiceFactory,
						_employeeWageParametersFactory,
						_employeeJournalFactory,
						_subdivisionJournalFactory,
						_employeePostsJournalFactory,
						_cashDistributionCommonOrganisationProvider,
						_subdivisionService,
						_emailServiceSettingAdapter,
						_wageCalculationRepository,
						_employeeRepository,
						_validationContextFactory,
						_phonesViewModelFactory,
						ServicesConfig.CommonServices,
						UnitOfWorkFactory.GetDefaultFactory);
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
					
			return new EmployeesJournalViewModel(
				forwarderFilter,
				_authorizationServiceFactory,
				_employeeWageParametersFactory,
				_employeeJournalFactory,
				_subdivisionJournalFactory,
				_employeePostsJournalFactory,
				_cashDistributionCommonOrganisationProvider,
				_subdivisionService,
				_emailServiceSettingAdapter,
				_wageCalculationRepository,
				_employeeRepository,
				_validationContextFactory,
				_phonesViewModelFactory,
				ServicesConfig.CommonServices,
				UnitOfWorkFactory.GetDefaultFactory);
		}
	}
}
