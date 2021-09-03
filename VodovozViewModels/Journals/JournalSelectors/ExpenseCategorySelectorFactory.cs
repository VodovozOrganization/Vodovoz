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
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IExpenseCategorySelectorFactory expenseCategorySelectorFactory)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_filter = filterViewModel;
			_fileChooserProvider = fileChooserProvider;
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_expenseCategorySelectorFactory =
				expenseCategorySelectorFactory ?? throw new ArgumentNullException(nameof(expenseCategorySelectorFactory));
		}

		protected readonly ICommonServices _commonServices;
		protected readonly ExpenseCategoryJournalFilterViewModel _filter;
		protected readonly IFileChooserProvider _fileChooserProvider;
		protected readonly IEmployeeJournalFactory _employeeJournalFactory;
		protected readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		protected readonly IExpenseCategorySelectorFactory _expenseCategorySelectorFactory;

		public Type EntityType => typeof(ExpenseCategory);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			ExpenseCategoryJournalViewModel selectorViewModel = new ExpenseCategoryJournalViewModel(
				_filter,
				UnitOfWorkFactory.GetDefaultFactory,
				_commonServices,
				_fileChooserProvider,
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
