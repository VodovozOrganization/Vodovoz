using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface ICarModelJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateCarModelAutocompleteSelectorFactory(
			CarModelJournalFilterViewModel filter = null, bool multipleSelect = false);
	}
}
