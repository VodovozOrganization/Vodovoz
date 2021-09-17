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
	public partial class UnclosedAdvancesFilter : RepresentationFilterBase<UnclosedAdvancesFilter>
	{
		protected override void ConfigureWithUow()
		{
			var employeeFactory = new EmployeeJournalFactory();
			evmeAccountable.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeAccountable.Changed += (sender, e) => OnRefiltered();
			yAdvancePeriod.PeriodChanged += (sender, e) => OnRefiltered();
			yentryExpense.Changed += (sender, e) => OnRefiltered();
			yentryExpense.ItemsQuery = new CategoryRepository(new ParametersProvider()).ExpenseCategoriesQuery();
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
			get { return evmeAccountable.Subject as Employee; }
			set {
				evmeAccountable.Subject = value;
				evmeAccountable.Sensitive = false;
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
	}
}
