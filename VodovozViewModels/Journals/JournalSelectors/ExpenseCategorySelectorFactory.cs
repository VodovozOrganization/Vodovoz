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
using Vodovoz.ViewModels.TempAdapters;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
    public class ExpenseCategorySelectorFactory : IEntitySelectorFactory
    {
        public ExpenseCategorySelectorFactory(ICommonServices commonServices,
	        ExpenseCategoryJournalFilterViewModel filterViewModel,
	        IFileChooserProvider fileChooserProvider,
	        IEmployeeJournalFactory employeeJournalFactory,
	        ISubdivisionJournalFactory subdivisionJournalFactory, IExpenseCategorySelectorFactory expenseCategorySelectorFactory)
        {
            this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            filter = filterViewModel;
            this.fileChooserProvider = fileChooserProvider;
            _employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
            _subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
            _expenseCategorySelectorFactory =
	            expenseCategorySelectorFactory ?? throw new ArgumentNullException(nameof(expenseCategorySelectorFactory));
        }

        protected readonly ICommonServices commonServices;
        protected readonly ExpenseCategoryJournalFilterViewModel filter;
        protected readonly IFileChooserProvider fileChooserProvider;
        protected readonly IEmployeeJournalFactory _employeeJournalFactory;
        protected readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
        protected readonly IExpenseCategorySelectorFactory _expenseCategorySelectorFactory;

        public Type EntityType => typeof(ExpenseCategory);

        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
	        ExpenseCategoryJournalViewModel selectorViewModel = new ExpenseCategoryJournalViewModel(
                filter,
                UnitOfWorkFactory.GetDefaultFactory,
                commonServices,
                fileChooserProvider,
                _employeeJournalFactory,
                _subdivisionJournalFactory,
                _expenseCategorySelectorFactory
            )
            {
                SelectionMode = JournalSelectionMode.Single
            };

            return selectorViewModel;
        }
    }
}
