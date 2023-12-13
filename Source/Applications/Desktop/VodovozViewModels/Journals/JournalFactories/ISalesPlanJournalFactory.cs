using QS.Navigation;
using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface ISalesPlanJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateSalesPlanAutocompleteSelectorFactory(INavigationManager navigationManager);
	}
}
