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
    public class IncomeCategorySelectorFactory : IEntitySelectorFactory
    {
        public IncomeCategorySelectorFactory(ICommonServices commonServices, 
            IncomeCategoryJournalFilterViewModel filterViewModel,
            IFileChooserProvider fileChooserProvider
            )
        {
            this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            filter = filterViewModel;
            this.fileChooserProvider = fileChooserProvider;
        }

        protected readonly ICommonServices commonServices;
        protected readonly IncomeCategoryJournalFilterViewModel filter;
        protected readonly IFileChooserProvider fileChooserProvider;

        public Type EntityType => typeof(IncomeCategory);

        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
            IncomeCategoryJournalViewModel selectorViewModel = new IncomeCategoryJournalViewModel(
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