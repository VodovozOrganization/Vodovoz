using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountableDebtsFilter : RepresentationFilterBase<AccountableDebtsFilter>
	{
		protected override void ConfigureWithUow()
		{
			entryreferenceExpense.ItemsQuery = Repository.Cash.CategoryRepository.ExpenseCategoriesQuery();
		}

		public AccountableDebtsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public AccountableDebtsFilter()
		{
			this.Build();
		}

		public ExpenseCategory RestrictExpenseCategory {
			get { return entryreferenceExpense.Subject as ExpenseCategory; }
			set {
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

