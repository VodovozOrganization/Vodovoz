using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IUserJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateSelectUserAutocompleteSelectorFactory();
	}
}
