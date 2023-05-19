using Autofac;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
	public class IncomeCategoryAutoCompleteSelectorFactory :
		IncomeCategorySelectorFactory, IEntityAutocompleteSelectorFactory
	{
		public IncomeCategoryAutoCompleteSelectorFactory(
			ILifetimeScope lifetimeScope)
			: base(lifetimeScope)
		{
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var selectorViewModel = _lifetimeScope.Resolve<IncomeCategoryJournalViewModel>();

			selectorViewModel.SelectionMode = JournalSelectionMode.Single;

			return selectorViewModel;
		}
	}
}
