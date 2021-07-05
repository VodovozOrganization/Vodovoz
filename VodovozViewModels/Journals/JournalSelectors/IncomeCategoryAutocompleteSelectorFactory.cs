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
    public class IncomeCategoryAutoCompleteSelectorFactory:
        IncomeCategorySelectorFactory, IEntityAutocompleteSelectorFactory
    {
	    public IncomeCategoryAutoCompleteSelectorFactory(
		    IncomeCategoryJournalActionsViewModel journalActionsViewModel,
		    ICommonServices commonServices,
		    IncomeCategoryJournalFilterViewModel filterViewModel,
		    IFileChooserProvider fileChooserProvider)
		    : base(journalActionsViewModel, commonServices, filterViewModel, fileChooserProvider)
	    {
		    
	    }

        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
            IncomeCategoryJournalViewModel selectorViewModel = new IncomeCategoryJournalViewModel(
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