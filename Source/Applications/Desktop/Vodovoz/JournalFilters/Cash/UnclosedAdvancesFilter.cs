using QS.Dialog;
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
	public partial class UnclosedAdvancesFilter : RepresentationFilterBase<UnclosedAdvancesFilter>, ISingleUoWDialog
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
	}
}
