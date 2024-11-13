using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.TempAdapters
{
	public interface IEmployeeJournalFactory
	{
		void SetEmployeeFilterViewModel(EmployeeFilterViewModel filter);
		IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateWorkingDriverEmployeeAutocompleteSelectorFactory(bool restrictedCategory = false);
		EmployeesJournalViewModel CreateWorkingDriverEmployeeJournal(bool restrictedCategory = false);
		IEntityAutocompleteSelectorFactory CreateWorkingForwarderEmployeeAutocompleteSelectorFactory(bool restrictedCategory = false);
		EmployeesJournalViewModel CreateWorkingForwarderEmployeeJournal(bool restrictedCategory = false);
		EmployeesJournalViewModel CreateEmployeesJournal(EmployeeFilterViewModel filterViewModel = null);
		IEntityAutocompleteSelectorFactory CreateWorkingOfficeEmployeeAutocompleteSelectorFactory(bool restrictedCategory = false);
		/// <summary>
		/// Журнал с фильтром по работающим сотрудникам любой категории
		/// </summary>
		/// <returns></returns>
		IEntityAutocompleteSelectorFactory CreateWorkingEmployeeAutocompleteSelectorFactory();
	}
}
