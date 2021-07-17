using QS.Project.Journal.EntitySelector;

namespace Vodovoz.TempAdapters
{
	public interface IEmployeeJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateWorkingForwarderEmployeeAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();
	}
}