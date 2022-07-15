using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IOrganizationJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateOrganizationAutocompleteSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateOrganizationsForAvangardPaymentsAutocompleteSelectorFactory();
	}
}
