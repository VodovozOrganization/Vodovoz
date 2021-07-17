using QS.DomainModel.UoW;
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
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() => new EmployeesJournalViewModel(_employeeJournalFilter ?? new EmployeeFilterViewModel(),
				UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices));
		}

		public IEntityAutocompleteSelectorFactory CreateWorkingDriverEmployeeAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() =>
				{
					var driverFilter = new EmployeeFilterViewModel
					{
						HidenByDefault = true,
						Status = EmployeeStatus.IsWorking,
						Category = EmployeeCategory.driver
					};
					
					return new EmployeesJournalViewModel(driverFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
				}
			);
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
						Status = EmployeeStatus.IsWorking,
						Category = EmployeeCategory.office
					};
					
					return new EmployeesJournalViewModel(officeFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
				}
			);
		}
		
		public IEntityAutocompleteSelectorFactory CreateWorkingForwarderEmployeeAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
				typeof(Employee),
				() =>
				{
					var forwarderFilter = new EmployeeFilterViewModel
					{
						HidenByDefault = true,
					};
					
					forwarderFilter.SetAndRefilterAtOnce(
						x => x.Status = EmployeeStatus.IsWorking,
						x => x.Category = EmployeeCategory.forwarder);
					
					return new EmployeesJournalViewModel(forwarderFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
				}
			);
		}
	}
}
