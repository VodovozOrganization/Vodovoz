using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IComplaintDetalizationAutocompleteSelectorFactory
	{
		IEntityAutocompleteSelectorFactory CreateComplaintDetalizationAutocompleteSelectorFactory(ComplaintDetalizationJournalFilterViewModel complainDetalizationFilter);
	}
}
