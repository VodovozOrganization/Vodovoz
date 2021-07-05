using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
    public class EmployeePostsJournalFactory : IEntityAutocompleteSelectorFactory
    {
	    private readonly EntitiesJournalActionsViewModel _journalActionsViewModel;
	    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICommonServices _commonServices;

        public EmployeePostsJournalFactory(
	        EntitiesJournalActionsViewModel journalActionsViewModel,
		    IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices)
        {
            _journalActionsViewModel = journalActionsViewModel ?? throw new ArgumentNullException(nameof(journalActionsViewModel));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
        }

        public Type EntityType => typeof(EmployeePost);


        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
	        var journal = new EmployeePostsJournalViewModel(_journalActionsViewModel, _unitOfWorkFactory, _commonServices)
	        {
		        SelectionMode = JournalSelectionMode.Single
	        };
	        
	        return journal;
        }

        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
            return CreateAutocompleteSelector(multipleSelect);
        }
    }
}