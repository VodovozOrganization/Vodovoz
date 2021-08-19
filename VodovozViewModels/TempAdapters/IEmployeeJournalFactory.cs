using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.TempAdapters
{
	public interface IEmployeeJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
		EmployeesJournalViewModel CreateWorkingDriverEmployeeJournal();
		IEntityAutocompleteSelectorFactory CreateWorkingForwarderEmployeeAutocompleteSelectorFactory();
		EmployeesJournalViewModel CreateWorkingForwarderEmployeeJournal();
		IEntityAutocompleteSelectorFactory CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();
	}
}