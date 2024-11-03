using QS.Dialog;
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
	public partial class UnclosedAdvancesFilter : RepresentationFilterBase<UnclosedAdvancesFilter>, ISingleUoWDialog, INotifyPropertyChanged
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
			evmeAccountable.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeAccountable.Changed += (sender, e) => OnRefiltered();
			yAdvancePeriod.PeriodChanged += (sender, e) => OnRefiltered();
		}

		public UnclosedAdvancesFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public UnclosedAdvancesFilter()
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
			get => evmeAccountable.Subject as Employee;
			set
			{
				evmeAccountable.Subject = value;
				evmeAccountable.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate
		{
			get => yAdvancePeriod.StartDateOrNull;
			set
			{
				yAdvancePeriod.StartDateOrNull = value;
				yAdvancePeriod.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate
		{
			get => yAdvancePeriod.EndDateOrNull;
			set
			{
				yAdvancePeriod.EndDateOrNull = value;
				yAdvancePeriod.Sensitive = false;
			}
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
