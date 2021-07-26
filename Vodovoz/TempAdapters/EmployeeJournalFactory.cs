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
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalSelectors;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.TempAdapters
{
	public class EmployeeJournalFactory : IEmployeeJournalFactory
	{
		private readonly EmployeeFilterViewModel _employeeJournalFilter;

		public EmployeeJournalFactory(EmployeeFilterViewModel employeeJournalFilter = null)
		{
			_employeeJournalFilter = employeeJournalFilter;
		}
		
		public IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory()
		{
			var authorizationServiceFactory = new AuthorizationServiceFactory();
			var employeeWageParametersFactory = new EmployeeWageParametersFactory();
			var employeeJournalFactory = new EmployeeJournalFactory();
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var employeePostsJournalFactory = new EmployeePostsJournalFactory();
        
			var cashDistributionCommonOrganisationProvider =
				new CashDistributionCommonOrganisationProvider(new OrganizationParametersProvider(new ParametersProvider()));
        
			var subdivisionService = SubdivisionParametersProvider.Instance;
			var emailServiceSettingAdapter = new EmailServiceSettingAdapter();
			var wageRepository = WageSingletonRepository.GetInstance();
			var employeeRepository = EmployeeSingletonRepository.GetInstance();
			var validationContextFactory = new ValidationContextFactory();
			var phonesViewModelFactory = new PhonesViewModelFactory(new PhoneRepository());
			
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() => new EmployeesJournalViewModel(
					_employeeJournalFilter ?? new EmployeeFilterViewModel(),
					authorizationServiceFactory,
					employeeWageParametersFactory,
					employeeJournalFactory,
					subdivisionJournalFactory,
					employeePostsJournalFactory,
					cashDistributionCommonOrganisationProvider,
					subdivisionService,
					emailServiceSettingAdapter,
					wageRepository,
					employeeRepository,
					validationContextFactory,
					phonesViewModelFactory,
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
			var authorizationServiceFactory = new AuthorizationServiceFactory();
			var employeeWageParametersFactory = new EmployeeWageParametersFactory();
			var employeeJournalFactory = new EmployeeJournalFactory();
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var employeePostsJournalFactory = new EmployeePostsJournalFactory();
        
			var cashDistributionCommonOrganisationProvider =
				new CashDistributionCommonOrganisationProvider(new OrganizationParametersProvider(new ParametersProvider()));
        
			var subdivisionService = SubdivisionParametersProvider.Instance;
			var emailServiceSettingAdapter = new EmailServiceSettingAdapter();
			var wageRepository = WageSingletonRepository.GetInstance();
			var employeeRepository = EmployeeSingletonRepository.GetInstance();
			var validationContextFactory = new ValidationContextFactory();
			var phonesViewModelFactory = new PhonesViewModelFactory(new PhoneRepository());
			
			var driverFilter = new EmployeeFilterViewModel
			{
				HidenByDefault = true,
				Status = EmployeeStatus.IsWorking,
				Category = EmployeeCategory.driver
			};
					
			return new EmployeesJournalViewModel(
				driverFilter,
				authorizationServiceFactory,
				employeeWageParametersFactory,
				employeeJournalFactory,
				subdivisionJournalFactory,
				employeePostsJournalFactory,
				cashDistributionCommonOrganisationProvider,
				subdivisionService,
				emailServiceSettingAdapter,
				wageRepository,
				employeeRepository,
				validationContextFactory,
				phonesViewModelFactory,
				ServicesConfig.CommonServices,
				UnitOfWorkFactory.GetDefaultFactory);
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingOfficeEmployeeAutocompleteSelectorFactory()
		{
			var authorizationServiceFactory = new AuthorizationServiceFactory();
			var employeeWageParametersFactory = new EmployeeWageParametersFactory();
			var employeeJournalFactory = new EmployeeJournalFactory();
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var employeePostsJournalFactory = new EmployeePostsJournalFactory();
        
			var cashDistributionCommonOrganisationProvider =
				new CashDistributionCommonOrganisationProvider(new OrganizationParametersProvider(new ParametersProvider()));
        
			var subdivisionService = SubdivisionParametersProvider.Instance;
			var emailServiceSettingAdapter = new EmailServiceSettingAdapter();
			var wageRepository = WageSingletonRepository.GetInstance();
			var employeeRepository = EmployeeSingletonRepository.GetInstance();
			var validationContextFactory = new ValidationContextFactory();
			var phonesViewModelFactory = new PhonesViewModelFactory(new PhoneRepository());
			
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
						authorizationServiceFactory,
						employeeWageParametersFactory,
						employeeJournalFactory,
						subdivisionJournalFactory,
						employeePostsJournalFactory,
						cashDistributionCommonOrganisationProvider,
						subdivisionService,
						emailServiceSettingAdapter,
						wageRepository,
						employeeRepository,
						validationContextFactory,
						phonesViewModelFactory,
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
			var authorizationServiceFactory = new AuthorizationServiceFactory();
			var employeeWageParametersFactory = new EmployeeWageParametersFactory();
			var employeeJournalFactory = new EmployeeJournalFactory();
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var employeePostsJournalFactory = new EmployeePostsJournalFactory();
        
			var cashDistributionCommonOrganisationProvider =
				new CashDistributionCommonOrganisationProvider(new OrganizationParametersProvider(new ParametersProvider()));
        
			var subdivisionService = SubdivisionParametersProvider.Instance;
			var emailServiceSettingAdapter = new EmailServiceSettingAdapter();
			var wageRepository = WageSingletonRepository.GetInstance();
			var employeeRepository = EmployeeSingletonRepository.GetInstance();
			var validationContextFactory = new ValidationContextFactory();
			var phonesViewModelFactory = new PhonesViewModelFactory(new PhoneRepository());
			
			var forwarderFilter = new EmployeeFilterViewModel
			{
				HidenByDefault = true,
			};
					
			forwarderFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.Category = EmployeeCategory.forwarder);
					
			return new EmployeesJournalViewModel(
				forwarderFilter,
				authorizationServiceFactory,
				employeeWageParametersFactory,
				employeeJournalFactory,
				subdivisionJournalFactory,
				employeePostsJournalFactory,
				cashDistributionCommonOrganisationProvider,
				subdivisionService,
				emailServiceSettingAdapter,
				wageRepository,
				employeeRepository,
				validationContextFactory,
				phonesViewModelFactory,
				ServicesConfig.CommonServices,
				UnitOfWorkFactory.GetDefaultFactory);
		}
	}
}
