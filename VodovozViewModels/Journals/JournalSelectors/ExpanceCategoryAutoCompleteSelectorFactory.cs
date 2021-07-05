using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Journals.JournalActionsViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
    public class ExpenseCategoryAutoCompleteSelectorFactory:
        ExpenseCategorySelectorFactory, IEntityAutocompleteSelectorFactory
    {
	    public ExpenseCategoryAutoCompleteSelectorFactory(
		    ExpenseCategoryJournalActionsViewModel journalActionsViewModel,
		    ICommonServices commonServices,
		    ExpenseCategoryJournalFilterViewModel filterViewModel,
		    IFileChooserProvider fileChooserProvider
	    )
		    : base(journalActionsViewModel, commonServices, filterViewModel, fileChooserProvider)
	    {
		    
	    }

        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
            ExpenseCategoryJournalViewModel selectorViewModel = new ExpenseCategoryJournalViewModel(
	            JournalActionsViewModel,
                Filter,
                UnitOfWorkFactory.GetDefaultFactory,
                CommonServices,
                FileChooserProvider)
            {
                SelectionMode = JournalSelectionMode.Single
            };
			
            return selectorViewModel;
        }
    }
}