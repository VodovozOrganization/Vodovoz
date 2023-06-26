using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;

namespace Vodovoz
{
	[ToolboxItem(true)]
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
			Build();
		}

		public FinancialExpenseCategory RestrictExpenseCategory
		{
			get => yentryExpense.Subject as FinancialExpenseCategory;
			set
			{
				yentryExpense.Subject = value;
				yentryExpense.Sensitive = false;
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
	}
}
