using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface IUndeliveryDetalizationJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateUndeliveryDetalizationAutocompleteSelectorFactory(UndeliveryDetalizationJournalFilterViewModel filterViewModel);
	}
}
