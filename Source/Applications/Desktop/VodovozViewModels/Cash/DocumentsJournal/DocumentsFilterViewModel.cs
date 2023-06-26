using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Cash;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Cash.DocumentsJournal
{
	public class DocumentsFilterViewModel : FilterViewModelBase<DocumentsFilterViewModel>, IJournalFilterViewModel
	{
		private readonly TimeSpan _defaultStartDateTimespan = TimeSpan.FromDays(-30);

		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _scope;
		private DialogViewModelBase _journalViewModel;
		private FinancialExpenseCategory _financialExpenseCategory;
		private FinancialIncomeCategory _financialIncomeCategory;
		private Subdivision _subdivision;
		private Employee _employee;
		private CashDocumentType? _cashDocumentType;
		private DateTime? _endDate;
		private DateTime? _startDate;

		public DocumentsFilterViewModel(
			INavigationManager navigationManager,
			ILifetimeScope scope)
		{
			StartDate = DateTime.Today.Add(_defaultStartDateTimespan);
			EndDate = DateTime.Today.AddDays(1);
			_navigationManager = navigationManager;
			_scope = scope;
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public bool IsShow { get; set; }

		public CashDocumentType? CashDocumentType
		{
			get => _cashDocumentType;
			set => UpdateFilterField(ref _cashDocumentType, value);
		}

		public Employee Employee
		{
			get => _employee;
			set => UpdateFilterField(ref _employee, value);
		}

		public Type[] DomainObjectsTypes { get; set; }

		public IEnumerable<Subdivision> AvailableSubdivisions { get; set; }

		public Subdivision Subdivision
		{
			get => _subdivision;
			set => UpdateFilterField(ref _subdivision, value);
		}

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => _financialExpenseCategory;
			set => UpdateFilterField(ref _financialExpenseCategory, value);
		}

		public FinancialIncomeCategory FinancialIncomeCategory
		{
			get => _financialIncomeCategory;
			set => UpdateFilterField(ref _financialIncomeCategory, value);
		}

		public DialogViewModelBase JournalViewModel
		{
			get => _journalViewModel;
			set
			{
				_journalViewModel = value;

				var employeeEntryViewModelBuilder = new CommonEEVMBuilderFactory<DocumentsFilterViewModel>(value, this, UoW, _navigationManager, _scope);

				EmployeeViewModel = employeeEntryViewModelBuilder
					.ForProperty(x => x.Employee)
					.UseViewModelDialog<EmployeeViewModel>()
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
						filter =>
						{
							filter.Status = EmployeeStatus.IsWorking;
						})
					.Finish();

				var cashIncomeCategoryViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<DocumentsFilterViewModel>(value, this, UoW, _navigationManager, _scope);

				FinancialIncomeCategoryViewModel = cashIncomeCategoryViewModelEntryViewModelBuilder
					.ForProperty(x => x.FinancialIncomeCategory)
					.UseViewModelDialog<IncomeCategoryViewModel>()
					.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
						filter =>
						{
							filter.RestrictFinancialSubtype = FinancialSubType.Income;
							filter.RestrictNodeSelectTypes.Add(typeof(FinancialIncomeCategory));
						})
					.Finish();

				var cashExpenseCategoryViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<DocumentsFilterViewModel>(value, this, UoW, _navigationManager, _scope);

				FinancialExpenseCategoryViewModel = cashExpenseCategoryViewModelEntryViewModelBuilder
					.ForProperty(x => x.FinancialExpenseCategory)
					.UseViewModelDialog<ExpenseCategoryViewModel>()
					.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
						filter =>
						{
							filter.RestrictFinancialSubtype = FinancialSubType.Expense;
							filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
						})
					.Finish();
			}
		}

		public IEntityEntryViewModel EmployeeViewModel { get; private set; }

		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; private set; }

		public IEntityEntryViewModel FinancialIncomeCategoryViewModel { get; private set; }
	}
}
