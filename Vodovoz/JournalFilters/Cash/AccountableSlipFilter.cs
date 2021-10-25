using System;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountableSlipFilter : RepresentationFilterBase<AccountableSlipFilter>, IAccountableSlipsFilter
	{
		protected override void ConfigureWithUow()
		{
			yentryExpense.ItemsQuery = new CategoryRepository(new ParametersProvider()).ExpenseCategoriesQuery();

			var employeeFactory = new EmployeeJournalFactory();
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
			this.Build();
		}

		public ExpenseCategory RestrictExpenseCategory {
			get { return yentryExpense.Subject as ExpenseCategory; }
			set {
				yentryExpense.Subject = value;
				yentryExpense.Sensitive = false;
			}
		}

		public Employee RestrictAccountable {
			get { return evmeEmployee.Subject as Employee; }
			set {
				evmeEmployee.Subject = value;
				evmeEmployee.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get { return dateperiod.StartDateOrNull; }
			set {
				dateperiod.StartDateOrNull = value;
				dateperiod.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return dateperiod.EndDateOrNull; }
			set {
				dateperiod.EndDateOrNull = value;
				dateperiod.Sensitive = false;
			}
		}

		public decimal? RestrictDebt => null;

		protected void OnYentryExpenseChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}
