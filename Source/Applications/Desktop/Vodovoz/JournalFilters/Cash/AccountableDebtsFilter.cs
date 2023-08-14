using QS.DomainModel.UoW;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSOrmProject.RepresentationModel;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountableDebtsFilter : RepresentationFilterBase<AccountableDebtsFilter>, INotifyPropertyChanged
	{
		private ITdiTab _journalTab;
		private FinancialExpenseCategory _fianncialExpenseCategory;

		public event PropertyChangedEventHandler PropertyChanged;

		public ITdiTab JournalTab
		{
			get => _journalTab;
			set
			{
				_journalTab = value;

				entryExpenseFinancialCategory.ViewModel = new LegacyEEVMBuilderFactory(value, UoW, Startup.MainWin.NavigationManager, Startup.AppDIContainer.BeginLifetimeScope())
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

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => _fianncialExpenseCategory;
			set
			{
				_fianncialExpenseCategory = value;
			}
		}

		public AccountableDebtsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public AccountableDebtsFilter()
		{
			Build();
		}

		protected void OnEntryreferenceExpenseChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

