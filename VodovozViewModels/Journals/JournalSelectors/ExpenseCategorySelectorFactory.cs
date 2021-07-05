using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Journals.JournalActionsViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
    public class ExpenseCategorySelectorFactory : IEntitySelectorFactory
    {
        public ExpenseCategorySelectorFactory(
	        ExpenseCategoryJournalActionsViewModel journalActionsViewModel,
	        ICommonServices commonServices, 
            ExpenseCategoryJournalFilterViewModel filterViewModel,
            IFileChooserProvider fileChooserProvider
        )
        {
            JournalActionsViewModel = journalActionsViewModel ?? throw new ArgumentNullException(nameof(journalActionsViewModel));
            CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            Filter = filterViewModel;
            FileChooserProvider = fileChooserProvider;
        }

        protected readonly ExpenseCategoryJournalActionsViewModel JournalActionsViewModel;
        protected readonly ICommonServices CommonServices;
        protected readonly ExpenseCategoryJournalFilterViewModel Filter;
        protected readonly IFileChooserProvider FileChooserProvider;

        public Type EntityType => typeof(ExpenseCategory);

        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
            
            ExpenseCategoryJournalViewModel selectorViewModel = new ExpenseCategoryJournalViewModel(
	            JournalActionsViewModel,
                Filter,
                UnitOfWorkFactory.GetDefaultFactory,
                CommonServices,
                FileChooserProvider
            )
            {
                SelectionMode = JournalSelectionMode.Single
            };

            return selectorViewModel;
        }
    }
}