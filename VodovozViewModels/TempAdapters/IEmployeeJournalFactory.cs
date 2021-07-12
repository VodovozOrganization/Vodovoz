using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Orders;

namespace Vodovoz.TempAdapters
{
	public interface IEmployeeJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();
	}
}