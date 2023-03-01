using QS.Project.Journal.EntitySelector;
using Vodovoz.Journals.FilterViewModels;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface IDistrictJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateDistrictAutocompleteSelectorFactory(DistrictJournalFilterViewModel districtJournalFilterViewModel = null);
		IEntityAutocompleteSelectorFactory CreateDistrictAutocompleteSelectorFactory(DistrictJournalFilterViewModel districtJournalFilterViewModel, bool enableDfaultButtons);
	}
}
