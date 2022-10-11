using System;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Parameters;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountableDebtsFilter : RepresentationFilterBase<AccountableDebtsFilter>
	{
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

