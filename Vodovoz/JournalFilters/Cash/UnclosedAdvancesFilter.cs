using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UnclosedAdvancesFilter : RepresentationFilterBase<UnclosedAdvancesFilter>
	{
		protected override void ConfigureWithUow()
		{
			var filter = new EmployeeFilter(UoW);
			filter.RestrictFired = false;
			yentryAccountable.RepresentationModel = new ViewModel.EmployeesVM(filter);
			yentryExpense.ItemsQuery = Repository.Cash.CategoryRepository.ExpenseCategoriesQuery();
			yAdvancePeriod.StartDateOrNull = DateTime.Today.AddMonths(-1);
			yAdvancePeriod.EndDateOrNull = DateTime.Today;
		}

		public UnclosedAdvancesFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public UnclosedAdvancesFilter()
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
			get { return yentryAccountable.Subject as Employee; }
			set {
				yentryAccountable.Subject = value;
				yentryAccountable.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get { return yAdvancePeriod.StartDateOrNull; }
			set {
				yAdvancePeriod.StartDateOrNull = value;
				yAdvancePeriod.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return yAdvancePeriod.EndDateOrNull; }
			set {
				yAdvancePeriod.EndDateOrNull = value;
				yAdvancePeriod.Sensitive = false;
			}
		}

		protected void OnYentryAccountableChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYentryExpenseChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYAdvancePeriodPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

