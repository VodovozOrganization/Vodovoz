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
    public class IncomeCategoryJournalFactory : IIncomeCategoryJournalFactory
    {
	    public IEntityAutocompleteSelectorFactory CreateIncomeCategoryAutocompleteSelector(
		    IncomeCategoryJournalFilterViewModel filterViewModel = null, string fileName = null, bool multipleSelect = false)
	    {
		    return new EntityAutocompleteSelectorFactory<IncomeCategoryJournalViewModel>(
			    typeof(IncomeCategory),
			    () => CreateIncomeCategoryJournal(filterViewModel, fileName, multipleSelect));
	    }

	    public IncomeCategoryJournalViewModel CreateIncomeCategoryJournal(
		    IncomeCategoryJournalFilterViewModel filterViewModel = null, string fileName = null, bool multipleSelect = false)
	    {
		    var fileChooserProvider = new FileChooser(fileName ?? "Категории прихода.csv");
		    var journalActions = new IncomeCategoryJournalActionsViewModel(ServicesConfig.InteractiveService, fileChooserProvider);
		    var employeeJournalFactory = new EmployeeJournalFactory();
		    var subdivisionJournalFactory = new SubdivisionJournalFactory();
	        
		    var journalViewModel = new IncomeCategoryJournalViewModel(
			    journalActions,
			    filterViewModel ?? new IncomeCategoryJournalFilterViewModel(),
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