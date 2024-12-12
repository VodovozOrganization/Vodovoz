using QS.DomainModel.UoW;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSOrmProject.RepresentationModel;
using System;
using System.ComponentModel;
using Autofac;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;

namespace Vodovoz
{
	[ToolboxItem(true)]
	public partial class AccountableSlipFilter : RepresentationFilterBase<AccountableSlipFilter>, IAccountableSlipsFilter, INotifyPropertyChanged
	{
		private ITdiTab _journalTab;
		private FinancialExpenseCategory _fianncialExpenseCategory;
		private ILifetimeScope _scope = Startup.AppDIContainer.BeginLifetimeScope();

		public event PropertyChangedEventHandler PropertyChanged;

		public ITdiTab JournalTab
		{
			get => _journalTab;
			set
			{
				_journalTab = value;

				entryExpenseFinancialCategory.ViewModel = new LegacyEEVMBuilderFactory(value, UoW, Startup.MainWin.NavigationManager, _scope)
					.ForEntity<FinancialExpenseCategory>()
					.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					})
					.Finish();

				entryExpenseFinancialCategory.ViewModel.ChangedByUser += (s, e) =>
				{
					FinancialExpenseCategory = entryExpenseFinancialCategory.ViewModel.Entity as FinancialExpenseCategory;
					OnRefiltered();
				};
			}
		}

		protected override void ConfigureWithUow()
		{
			var employeeFactory = new EmployeeJournalFactory(_scope);
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.Changed += (sender, args) => OnRefiltered();
			dateperiod.PeriodChanged += (sender, args) => OnRefiltered();
		}

		public AccountableSlipFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public AccountableSlipFilter()
		{
			Build();
		}

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => _fianncialExpenseCategory;
			set
			{
				_fianncialExpenseCategory = value;
			}
		}

		public Employee RestrictAccountable
		{
			get => evmeEmployee.Subject as Employee;
			set
			{
				evmeEmployee.Subject = value;
				evmeEmployee.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate
		{
			get => dateperiod.StartDateOrNull;
			set
			{
				dateperiod.StartDateOrNull = value;
				dateperiod.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate
		{
			get => dateperiod.EndDateOrNull;
			set
			{
				dateperiod.EndDateOrNull = value;
				dateperiod.Sensitive = false;
			}
		}

		public decimal? RestrictDebt => null;

		protected void OnYentryExpenseChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected override void OnDestroyed()
		{
			if(_scope != null)
			{
				_scope.Dispose();
				_scope = null;
			}
			base.OnDestroyed();
		}
	}
}
