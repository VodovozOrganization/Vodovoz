using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
    public class IncomeCategorySelectorFactory : IEntitySelectorFactory
    {
        public IncomeCategorySelectorFactory(
	        ICommonServices commonServices, 
            IncomeCategoryJournalFilterViewModel filterViewModel,
            IFileChooserProvider fileChooserProvider,
	        IEmployeeJournalFactory employeeJournalFactory,
	        ISubdivisionJournalFactory subdivisionJournalFactory)
        {
            this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            filter = filterViewModel;
            this.fileChooserProvider = fileChooserProvider;
            _employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
            _subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
        }

        protected readonly ICommonServices commonServices;
        protected readonly IncomeCategoryJournalFilterViewModel filter;
        protected readonly IFileChooserProvider fileChooserProvider;
        protected readonly IEmployeeJournalFactory _employeeJournalFactory;
        protected readonly ISubdivisionJournalFactory _subdivisionJournalFactory;

        public Type EntityType => typeof(IncomeCategory);

        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
            IncomeCategoryJournalViewModel selectorViewModel = new IncomeCategoryJournalViewModel(
                filter,
                UnitOfWorkFactory.GetDefaultFactory,
                commonServices,
                fileChooserProvider,
                _employeeJournalFactory,
                _subdivisionJournalFactory)
            {
                SelectionMode = JournalSelectionMode.Single
            };

            return selectorViewModel;
        }
    }
}