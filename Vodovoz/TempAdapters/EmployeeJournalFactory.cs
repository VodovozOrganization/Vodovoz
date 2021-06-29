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
		private readonly IJournalFilter _employeeJournalFilter;
		
		public EmployeeJournalFactory(IJournalFilter employeeJournalFilter = null)
		{
			_employeeJournalFilter = employeeJournalFilter;
		}
		public IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee), () =>
			{
				return new EmployeesJournalViewModel((_employeeJournalFilter as EmployeeFilterViewModel) ?? new EmployeeFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			});
		}
	}
}
