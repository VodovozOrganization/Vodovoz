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
		private readonly IUnitOfWorkFactory _uowFactory;

		public EmployeePostsJournalFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

        public IEntityAutocompleteSelectorFactory CreateEmployeePostsAutocompleteSelectorFactory(bool multipleSelect = false)
        {
	        return new EntityAutocompleteSelectorFactory<EmployeePostsJournalViewModel>(
		        typeof(EmployeePost),
		        () => new EmployeePostsJournalViewModel(
					_uowFactory,
			        ServicesConfig.CommonServices)
		        {
			        SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single
		        });
        }
    }
}
