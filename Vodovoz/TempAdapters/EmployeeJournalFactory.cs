using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals.JournalActionsViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
    public class EmployeeJournalFactory : IEmployeeJournalFactory
    {
	    private readonly EmployeesJournalActionsViewModel _journalActionsViewModel;
	    private readonly EmployeeFilterViewModel _journalFilter;
	    
	    public EmployeeJournalFactory(
		    EmployeesJournalActionsViewModel journalActionsViewModel,
		    EmployeeFilterViewModel journalFilter)
	    {
		    _journalActionsViewModel = journalActionsViewModel ?? throw new ArgumentNullException(nameof(journalActionsViewModel));
		    _journalFilter = journalFilter ?? throw new ArgumentNullException(nameof(journalFilter));
	    }
	    
        public IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory(bool multipleSelect = false)
        {
	        var factory = new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(
		        typeof(Employee),
		        () => new EmployeesJournalViewModel(
			        _journalActionsViewModel, _journalFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices)
		        {
			        SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single
		        });

	        return factory;
        }
    }
}