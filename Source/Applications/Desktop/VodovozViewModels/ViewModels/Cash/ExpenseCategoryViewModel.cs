using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class ExpenseCategoryViewModel : EntityTabViewModelBase<ExpenseCategory>
	{
		private readonly ILifetimeScope _scope;
		private FinancialCategoriesGroup _parentFinancialCategoriesGroup;

		public ExpenseCategoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IExpenseCategorySelectorFactory expenseCategorySelectorFactory,
			INavigationManager navigationManager,
			ILifetimeScope scope)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			ExpenseCategoryAutocompleteSelectorFactory =
				(expenseCategorySelectorFactory ?? throw new ArgumentNullException(nameof(expenseCategorySelectorFactory)))
				.CreateDefaultExpenseCategoryAutocompleteSelectorFactory();

			var employeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateEmployeeAutocompleteSelectorFactory();

			SubdivisionAutocompleteSelectorFactory =
				(subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory)))
			.CreateDefaultSubdivisionAutocompleteSelectorFactory(employeeSelectorFactory);

			if(Entity.FinancialCategoryGroupId.HasValue)
			{
				ParentFinancialCategoriesGroup = UoW.GetById<FinancialCategoriesGroup>(Entity.FinancialCategoryGroupId.Value);
			}

			var complaintDetalizationEntryViewModelBuilder = new CommonEEVMBuilderFactory<ExpenseCategoryViewModel>(this, this, UoW, NavigationManager, _scope);

			ParentFinancialCategoriesGroupViewModel = complaintDetalizationEntryViewModelBuilder
				.ForProperty(x => x.ParentFinancialCategoriesGroup)
				.UseViewModelDialog<FinancialCategoriesGroupViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictNodeTypes.Add(typeof(FinancialCategoriesGroup));
					}
				)
				.Finish();

			TabName = uowBuilder.IsNewEntity ? "Создание новой категории расхода" : $"{Entity.Title}";
			_scope = scope;
		}

		public IEntityAutocompleteSelectorFactory SubdivisionAutocompleteSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory ExpenseCategoryAutocompleteSelectorFactory { get; }
		public IEntityEntryViewModel ParentFinancialCategoriesGroupViewModel { get; }

		public FinancialCategoriesGroup ParentFinancialCategoriesGroup
		{
			get => _parentFinancialCategoriesGroup;
			set
			{
				if(SetField(ref _parentFinancialCategoriesGroup, value))
				{
					Entity.FinancialCategoryGroupId = value?.Id;
				}
			}
		}

		public bool IsArchive
		{
			get => Entity.IsArchive;
			set => Entity.SetIsArchiveRecursively(value);
		}
	}
}
