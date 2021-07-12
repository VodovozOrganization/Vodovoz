using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface ISubdivisionJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateSubdivisionAutocompleteSelectorFactory(IEntityAutocompleteSelectorFactory employeeSelectorFactory);
	}
}
