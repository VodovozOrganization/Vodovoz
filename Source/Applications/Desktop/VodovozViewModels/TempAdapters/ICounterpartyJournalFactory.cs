using QS.Project.Journal.EntitySelector;

namespace Vodovoz.TempAdapters
{
	public interface ICounterpartyJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateCounterpartyAutocompleteSelectorFactory();
	}
}
