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
	public class IncomeCategorySelectorFactory : IEntitySelectorFactory
	{
		public IncomeCategorySelectorFactory(
			ICommonServices commonServices,
			IncomeCategoryJournalFilterViewModel filterViewModel,
			IFileChooserProvider fileChooserProvider,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IIncomeCategorySelectorFactory incomeCategorySelectorFactory)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_filter = filterViewModel;
			_fileChooserProvider = fileChooserProvider;
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_incomeCategorySelectorFactory =
				incomeCategorySelectorFactory ?? throw new ArgumentNullException(nameof(incomeCategorySelectorFactory));
		}

		protected readonly ICommonServices _commonServices;
		protected readonly IncomeCategoryJournalFilterViewModel _filter;
		protected readonly IFileChooserProvider _fileChooserProvider;
		protected readonly IEmployeeJournalFactory _employeeJournalFactory;
		protected readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		protected readonly IIncomeCategorySelectorFactory _incomeCategorySelectorFactory;

		public Type EntityType => typeof(IncomeCategory);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			IncomeCategoryJournalViewModel selectorViewModel = new IncomeCategoryJournalViewModel(
				_filter,
				UnitOfWorkFactory.GetDefaultFactory,
				_commonServices,
				_fileChooserProvider,
				_employeeJournalFactory,
				_subdivisionJournalFactory,
				_incomeCategorySelectorFactory)
			{
				SelectionMode = JournalSelectionMode.Single
			};

			return selectorViewModel;
		}
	}
}
