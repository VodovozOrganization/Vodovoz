using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class IncomeCategoryViewModel : EntityTabViewModelBase<IncomeCategory>
	{
		public IncomeCategoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IIncomeCategorySelectorFactory incomeCategorySelectorFactory
		) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			IncomeCategoryAutocompleteSelectorFactory =
				incomeCategorySelectorFactory?.CreateDefaultIncomeCategoryAutocompleteSelectorFactory()
				?? throw new ArgumentNullException(nameof(incomeCategorySelectorFactory));

			var employeeAutocompleteSelector = employeeJournalFactory?.CreateEmployeeAutocompleteSelectorFactory()
			                                   ?? throw new ArgumentNullException(nameof(employeeJournalFactory));

			SubdivisionAutocompleteSelectorFactory =
				subdivisionJournalFactory?.CreateDefaultSubdivisionAutocompleteSelectorFactory(employeeAutocompleteSelector)
				?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));

			TabName = uowBuilder.IsNewEntity ? "Создание новой категории дохода" : $"{Entity.Title}";
		}

		public IEntityAutocompleteSelectorFactory SubdivisionAutocompleteSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory IncomeCategoryAutocompleteSelectorFactory { get; }

		public bool IsArchive
		{
			get => Entity.IsArchive;
			set => Entity.SetIsArchiveRecursively(value);
		}
	}
}
