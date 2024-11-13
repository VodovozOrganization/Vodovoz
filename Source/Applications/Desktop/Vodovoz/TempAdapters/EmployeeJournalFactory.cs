using System;
using Autofac;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.TempAdapters
{
	public class EmployeeJournalFactory : IEmployeeJournalFactory, IDisposable
	{
		private EmployeeFilterViewModel _employeeJournalFilter;
		private readonly IAuthorizationServiceFactory _authorizationServiceFactory;
		private ILifetimeScope _scope;

		public EmployeeJournalFactory(
			ILifetimeScope scope,
			EmployeeFilterViewModel employeeJournalFilter = null)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_employeeJournalFilter = employeeJournalFilter;
			_authorizationServiceFactory = new AuthorizationServiceFactory();
		}

		public EmployeesJournalViewModel CreateEmployeesJournal(EmployeeFilterViewModel filterViewModel = null)
		{
			return new EmployeesJournalViewModel(
				filterViewModel ?? _employeeJournalFilter ?? new EmployeeFilterViewModel(),
				_authorizationServiceFactory,
				ServicesConfig.CommonServices,
				ServicesConfig.UnitOfWorkFactory,
				_scope,
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

		public IEntityAutocompleteSelectorFactory CreateWorkingDriverEmployeeAutocompleteSelectorFactory(bool restrictedCategory = false)
		{
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() => CreateWorkingDriverEmployeeJournal(restrictedCategory)
			);
		}
		
		public EmployeesJournalViewModel CreateWorkingDriverEmployeeJournal(bool restrictedCategory = false)
		{
			var driverFilter = new EmployeeFilterViewModel
			{
				HidenByDefault = true,
			};

			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x =>
				{
					const EmployeeCategory driver = EmployeeCategory.driver;

					if(restrictedCategory)
					{
						x.RestrictCategory = driver;
					}
					else
					{
						x.Category = driver;
					}
				});

			return CreateEmployeesJournal(driverFilter);
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingOfficeEmployeeAutocompleteSelectorFactory(bool restrictedCategory = false)
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
						x =>
						{
							const EmployeeCategory office = EmployeeCategory.office;

							if(restrictedCategory)
							{
								x.RestrictCategory = office;
							}
							else
							{
								x.Category = office;
							}
						});
					
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
						ServicesConfig.UnitOfWorkFactory,
						_scope,
						Startup.MainWin.NavigationManager
					);
				}
			);
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingForwarderEmployeeAutocompleteSelectorFactory(bool restrictedCategory = false)
		{
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() => CreateWorkingForwarderEmployeeJournal(restrictedCategory)
			);
		}

		public EmployeesJournalViewModel CreateWorkingForwarderEmployeeJournal(bool restrictedCategory = false)
		{
			var forwarderFilter = new EmployeeFilterViewModel
			{
				HidenByDefault = true,
			};
					
			forwarderFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x =>
				{
					const EmployeeCategory forwarder = EmployeeCategory.forwarder;

					if(restrictedCategory)
					{
						x.RestrictCategory = forwarder;
					}
					else
					{
						x.Category = forwarder;
					}
				});
					
			return CreateEmployeesJournal(forwarderFilter);
		}

		public void Dispose()
		{
			_scope = null;
		}
	}
}
