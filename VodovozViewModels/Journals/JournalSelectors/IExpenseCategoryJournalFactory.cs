using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
	public interface IExpenseCategoryJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateExpenseCategoryAutocompleteSelector(
			ExpenseCategoryJournalFilterViewModel filterViewModel = null, string fileName = null, bool multipleSelect = false);
		ExpenseCategoryJournalViewModel CreateExpenseCategoryJournal(
			ExpenseCategoryJournalFilterViewModel filterViewModel = null, string fileName = null, bool multipleSelect = false);
	}
}