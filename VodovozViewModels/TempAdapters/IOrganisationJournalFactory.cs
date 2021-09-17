using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IOrganisationJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateOrganisationAutocompleteSelectorFactory();
	}
}
