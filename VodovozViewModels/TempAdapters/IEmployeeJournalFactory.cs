using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.TempAdapters
{
	public interface IEmployeeJournalFactory
	{
		void SetEmployeeFilterViewModel(EmployeeFilterViewModel filter);
		IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
		EmployeesJournalViewModel CreateWorkingDriverEmployeeJournal();
		IEntityAutocompleteSelectorFactory CreateWorkingForwarderEmployeeAutocompleteSelectorFactory();
		EmployeesJournalViewModel CreateWorkingForwarderEmployeeJournal();
		IEntityAutocompleteSelectorFactory CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();
	}
}
