using QS.DomainModel.UoW;
using QS.Tdi;
using QSOrmProject.RepresentationModel;
using System;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Parameters;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountableDebtsFilter : RepresentationFilterBase<AccountableDebtsFilter>
	{
		private ITdiTab _journalTab;

		public ITdiTab JournalTab
		{
			get => _journalTab;
			set
			{
				_journalTab = value;
				
			}
		}

		protected override void ConfigureWithUow()
		{
			entryreferenceExpense.ItemsQuery = new CategoryRepository(new ParametersProvider()).ExpenseCategoriesQuery();
		}

		public AccountableDebtsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public AccountableDebtsFilter()
		{
			Build();
		}

		public FinancialExpenseCategory RestrictExpenseCategory
		{
			get => entryreferenceExpense.Subject as FinancialExpenseCategory;
			set
			{
				entryreferenceExpense.Subject = value;
				entryreferenceExpense.Sensitive = false;
			}
		}

		protected void OnEntryreferenceExpenseChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

