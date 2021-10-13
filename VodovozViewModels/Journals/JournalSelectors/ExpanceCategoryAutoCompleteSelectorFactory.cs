using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.TempAdapters;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
	public class ExpenseCategoryAutoCompleteSelectorFactory :
		ExpenseCategorySelectorFactory, IEntityAutocompleteSelectorFactory
	{
		public ExpenseCategoryAutoCompleteSelectorFactory(
			ICommonServices commonServices,
			ExpenseCategoryJournalFilterViewModel filterViewModel,
			IFileChooserProvider fileChooserProvider,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IExpenseCategorySelectorFactory expenseCategorySelectorFactory)
			: base(commonServices,
				filterViewModel,
				fileChooserProvider,
				employeeJournalFactory,
				subdivisionJournalFactory,
				expenseCategorySelectorFactory)
		{
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			ExpenseCategoryJournalViewModel selectorViewModel = new ExpenseCategoryJournalViewModel(
				_filter,
				UnitOfWorkFactory.GetDefaultFactory,
				_commonServices,
				_fileChooserProvider,
				_employeeJournalFactory,
				_subdivisionJournalFactory,
				_expenseCategorySelectorFactory)
			{
				SelectionMode = JournalSelectionMode.Single
			};

			return selectorViewModel;
		}
	}
}
