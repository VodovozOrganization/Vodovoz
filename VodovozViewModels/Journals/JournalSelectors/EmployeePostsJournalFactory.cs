using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
    public class EmployeePostsJournalFactory : IEmployeePostsJournalFactory
    {
        public IEntityAutocompleteSelectorFactory CreateEmployeePostsAutocompleteSelectorFactory(bool multipleSelect = false)
        {
	        return new EntityAutocompleteSelectorFactory<EmployeePostsJournalViewModel>(
		        typeof(EmployeePost),
		        () => new EmployeePostsJournalViewModel(
			        UnitOfWorkFactory.GetDefaultFactory,
			        ServicesConfig.CommonServices)
		        {
			        SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single
		        });
        }
    }
}