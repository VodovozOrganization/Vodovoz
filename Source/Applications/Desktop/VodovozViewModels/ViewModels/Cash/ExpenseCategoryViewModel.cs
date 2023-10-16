using Autofac;
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
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class ExpenseCategoryViewModel : EntityTabViewModelBase<ExpenseCategory>
	{
		private readonly ILifetimeScope _scope;
		private FinancialExpenseCategory _financialExpenseCategory;

		public ExpenseCategoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
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

			UpdateFinancialExpenseCategory();

			var complaintDetalizationEntryViewModelBuilder = new CommonEEVMBuilderFactory<ExpenseCategoryViewModel>(this, this, UoW, NavigationManager, _scope);

			ParentFinancialCategoriesGroupViewModel = complaintDetalizationEntryViewModelBuilder
				.ForProperty(x => x.FinancialExpenseCategory)
				.UseViewModelDialog<FinancialExpenseCategoryViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.ExcludeFinancialGroupsIds.Add(1);
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					}
				)
				.Finish();

			TabName = uowBuilder.IsNewEntity ? "Создание новой категории расхода" : $"{Entity.Title}";
			_scope = scope;

			Entity.PropertyChanged += OnEntityPropertyChanged;
			BuildSubdivisionViewModel();
		}

		private void BuildSubdivisionViewModel()
		{
			SubdivisionViewModel = new CommonEEVMBuilderFactory<ExpenseCategoryViewModel>(this, this, UoW, NavigationManager, _scope)
				.ForProperty<Subdivision>(x => x.Entity.Subdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.Finish();
		}

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.FinancialExpenseCategoryId))
			{
				UpdateFinancialExpenseCategory();
			}
		}

		private void UpdateFinancialExpenseCategory()
		{
			if(Entity.FinancialExpenseCategoryId != null)
			{
				FinancialExpenseCategory = UoW.GetById<FinancialExpenseCategory>(Entity.FinancialExpenseCategoryId.Value);
			}
			else
			{
				FinancialExpenseCategory = null;
			}
		}

		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }
		public IEntityAutocompleteSelectorFactory ExpenseCategoryAutocompleteSelectorFactory { get; }
		public IEntityEntryViewModel ParentFinancialCategoriesGroupViewModel { get; }

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => _financialExpenseCategory;
			set
			{
				if(SetField(ref _financialExpenseCategory, value))
				{
					Entity.FinancialExpenseCategoryId = value?.Id;
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
