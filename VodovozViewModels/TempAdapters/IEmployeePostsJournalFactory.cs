using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IEmployeePostsJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateEmployeePostsAutocompleteSelectorFactory(bool multipleSelect = false);
	}
}