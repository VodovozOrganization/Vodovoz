using QS.Project.Journal.EntitySelector;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface ISalesPlanJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateSalesPlanAutocompleteSelectorFactory(INomenclatureJournalFactory nomenclatureSelectorFactory);
	}
}
