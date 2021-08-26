using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Journals.JournalActionsViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalSelectors;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.TempAdapters
{
    public class ExpenseCategoryJournalFactory: IExpenseCategoryJournalFactory
    {
        public IEntityAutocompleteSelectorFactory CreateExpenseCategoryAutocompleteSelector(
	        ExpenseCategoryJournalFilterViewModel filterViewModel = null, string fileName = null, bool multipleSelect = false)
        {
	        return new EntityAutocompleteSelectorFactory<ExpenseCategoryJournalViewModel>(
		        typeof(ExpenseCategory),
		        () => CreateExpenseCategoryJournal(filterViewModel, fileName, multipleSelect));
        }

        public ExpenseCategoryJournalViewModel CreateExpenseCategoryJournal(
	        ExpenseCategoryJournalFilterViewModel filterViewModel = null, string fileName = null, bool multipleSelect = false)
        {
	        var fileChooserProvider = new FileChooser(fileName ?? "Категории расхода.csv");
	        var journalActions = new ExpenseCategoryJournalActionsViewModel(ServicesConfig.InteractiveService, fileChooserProvider);
	        var employeeJournalFactory = new EmployeeJournalFactory();
	        var subdivisionJournalFactory = new SubdivisionJournalFactory();
	        
	        var journalViewModel = new ExpenseCategoryJournalViewModel(
		        journalActions,
		        filterViewModel ?? new ExpenseCategoryJournalFilterViewModel(),
		        UnitOfWorkFactory.GetDefaultFactory,
		        ServicesConfig.CommonServices,
		        employeeJournalFactory,
		        subdivisionJournalFactory,
		        this)
	        {
		        SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single
	        };
			
	        return journalViewModel;
        }
    }
}