using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

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
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee), () =>
			{
				return new EmployeesJournalViewModel(_employeeJournalFilter ?? new EmployeeFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			});
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingDriverEmployeeAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee), () =>
			{
				EmployeeFilterViewModel employeeFilterViewModel = new EmployeeFilterViewModel()
				{
					HidenByDefault = true,
					Status = EmployeeStatus.IsWorking,
					Category = EmployeeCategory.driver
				};
				return new EmployeesJournalViewModel(employeeFilterViewModel, UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices);
			});
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingOfficeEmployeeAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee), () =>
			{
				EmployeeFilterViewModel employeeFilterViewModel = new EmployeeFilterViewModel()
				{
					HidenByDefault = true,
					Status = EmployeeStatus.IsWorking,
					Category = EmployeeCategory.office
				};
				return new EmployeesJournalViewModel(employeeFilterViewModel, UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices);
			});
		}
	}
}
