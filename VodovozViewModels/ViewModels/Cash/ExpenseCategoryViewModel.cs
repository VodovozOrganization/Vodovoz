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
	public class ExpenseCategoryViewModel : EntityTabViewModelBase<ExpenseCategory>
	{
		public ExpenseCategoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IExpenseCategorySelectorFactory expenseCategorySelectorFactory
		) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			ExpenseCategoryAutocompleteSelectorFactory =
				(expenseCategorySelectorFactory ?? throw new ArgumentNullException(nameof(expenseCategorySelectorFactory)))
				.CreateDefaultExpenseCategoryAutocompleteSelectorFactory();

			var employeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateEmployeeAutocompleteSelectorFactory();

			SubdivisionAutocompleteSelectorFactory =
				(subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory)))
				.CreateDefaultSubdivisionAutocompleteSelectorFactory(employeeSelectorFactory);

			TabName = uowBuilder.IsNewEntity ? "Создание новой категории расхода" : $"{Entity.Title}";
		}

		public IEntityAutocompleteSelectorFactory SubdivisionAutocompleteSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory ExpenseCategoryAutocompleteSelectorFactory { get; }

		public bool IsArchive
		{
			get => Entity.IsArchive;
			set => Entity.SetIsArchiveRecursively(value);
		}
	}
}
