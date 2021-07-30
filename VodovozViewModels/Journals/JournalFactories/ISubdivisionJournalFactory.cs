using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface ISubdivisionJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateSubdivisionAutocompleteSelectorFactory(IEntityAutocompleteSelectorFactory employeeSelectorFactory, 
			ISalesPlanJournalFactory salesPlanJournalFactory, INomenclatureSelectorFactory nomenclatureSelectorFactory);
	}
}
