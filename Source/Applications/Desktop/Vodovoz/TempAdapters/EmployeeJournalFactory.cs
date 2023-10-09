using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.TempAdapters
{
	public class EmployeeJournalFactory : IEmployeeJournalFactory
	{
		private EmployeeFilterViewModel _employeeJournalFilter;
		private readonly IAuthorizationServiceFactory _authorizationServiceFactory;

		public EmployeeJournalFactory(EmployeeFilterViewModel employeeJournalFilter = null)
		{
			_employeeJournalFilter = employeeJournalFilter;
			_authorizationServiceFactory = new AuthorizationServiceFactory();
		}

		public EmployeesJournalViewModel CreateEmployeesJournal(EmployeeFilterViewModel filterViewModel = null)
		{
			return new EmployeesJournalViewModel(
				filterViewModel ?? _employeeJournalFilter ?? new EmployeeFilterViewModel(),
				_authorizationServiceFactory,
				ServicesConfig.CommonServices,
				UnitOfWorkFactory.GetDefaultFactory,
				Startup.AppDIContainer,
				Startup.MainWin.NavigationManager
			);
		}

		public void SetEmployeeFilterViewModel(EmployeeFilterViewModel filter)
		{
			_employeeJournalFilter = filter;
		}
		
		public IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory()
		{
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
						Startup.AppDIContainer,
						Startup.MainWin.NavigationManager
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
