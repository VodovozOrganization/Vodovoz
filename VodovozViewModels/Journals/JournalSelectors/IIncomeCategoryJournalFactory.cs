using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
	public interface IIncomeCategoryJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateIncomeCategoryAutocompleteSelector(
			IncomeCategoryJournalFilterViewModel filterViewModel = null, string fileName = null, bool multipleSelect = false);
		IncomeCategoryJournalViewModel CreateIncomeCategoryJournal(
			IncomeCategoryJournalFilterViewModel filterViewModel = null, string fileName = null, bool multipleSelect = false);
	}
}