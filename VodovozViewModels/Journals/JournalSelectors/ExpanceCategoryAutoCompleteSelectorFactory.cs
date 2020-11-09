using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
    public class ExpenseCategoryAutoCompleteSelectorFactory:
        ExpenseCategorySelectorFactory, IEntityAutocompleteSelectorFactory
    {
        public ExpenseCategoryAutoCompleteSelectorFactory(
            ICommonServices commonServices, 
            ExpenseCategoryJournalFilterViewModel filterViewModel,
            IFileChooserProvider fileChooserProvider
        ) 
            : base(commonServices, filterViewModel, fileChooserProvider) { }

        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
            ExpenseCategoryJournalViewModel selectorViewModel = new ExpenseCategoryJournalViewModel(
                filter,
                UnitOfWorkFactory.GetDefaultFactory,
                commonServices,
                fileChooserProvider)
            {
                SelectionMode = JournalSelectionMode.Single
            };
			
            return selectorViewModel;
        }
    }
}