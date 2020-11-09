using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
    public class ExpenseCategorySelectorFactory : IEntitySelectorFactory
    {
        public ExpenseCategorySelectorFactory(ICommonServices commonServices, 
            ExpenseCategoryJournalFilterViewModel filterViewModel,
            IFileChooserProvider fileChooserProvider
        )
        {
            this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            filter = filterViewModel;
            this.fileChooserProvider = fileChooserProvider;
        }

        protected readonly ICommonServices commonServices;
        protected readonly ExpenseCategoryJournalFilterViewModel filter;
        protected readonly IFileChooserProvider fileChooserProvider;

        public Type EntityType => typeof(ExpenseCategory);

        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
            
            ExpenseCategoryJournalViewModel selectorViewModel = new ExpenseCategoryJournalViewModel(
                filter,
                UnitOfWorkFactory.GetDefaultFactory,
                commonServices,
                fileChooserProvider
            )
            {
                SelectionMode = JournalSelectionMode.Single
            };

            return selectorViewModel;
        }
    }
}