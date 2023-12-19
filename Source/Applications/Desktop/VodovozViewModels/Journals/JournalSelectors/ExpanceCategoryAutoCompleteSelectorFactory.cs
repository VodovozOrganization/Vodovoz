using Autofac;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
	public class ExpenseCategoryAutoCompleteSelectorFactory :
		ExpenseCategorySelectorFactory, IEntityAutocompleteSelectorFactory
	{
		public ExpenseCategoryAutoCompleteSelectorFactory(ILifetimeScope lifetimeScope)
			: base(lifetimeScope)
		{
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var selectorViewModel = LifetimeScope.Resolve<ExpenseCategoryJournalViewModel>();

			selectorViewModel.SelectionMode = JournalSelectionMode.Single;

			return selectorViewModel;
		}
	}
}
