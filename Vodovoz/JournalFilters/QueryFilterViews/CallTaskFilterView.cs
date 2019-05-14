using System;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Tools;
using QS.Utilities;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters;
using Vodovoz.ViewModel;

namespace Vodovoz.JournalFilters.QueryFilterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallTaskFilterView : QueryFilterWidgetBase
	{
		public CallTaskFilter Filter { get; set; }

		public CallTaskFilterView()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Filter = new CallTaskFilter();
			EmployeesVM employeeVM = new EmployeesVM();
			comboboxDateType.ItemsEnum = typeof(TaskFilterDateType);
			comboboxDateType.Binding.AddBinding(Filter, x => x.DateType, w => w.SelectedItem).InitializeFromSource();
			employeeVM.Filter.RestrictCategory = EmployeeCategory.office;
			entryreferencevmEmployee.RepresentationModel = employeeVM;
			checkbuttonHideCompleted.Binding.AddBinding(Filter, x => x.HideCompleted, w => w.Active).InitializeFromSource();
			dateperiodpickerDateFilter.Binding.AddBinding(Filter, x => x.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			entryreferencevmEmployee.Binding.AddBinding(Filter, x => x.Employee, w => w.Subject).InitializeFromSource();
			dateperiodpickerDateFilter.Binding.AddBinding(Filter, x => x.EndDate, w => w.EndDateOrNull).InitializeFromSource();
		}

		public override IQueryFilter GetQueryFilter()
		{
			return Filter;
		}

		protected void OnCheckbuttonHideCompletedToggled(object sender, EventArgs e) => Refilter();
		protected void OnEntryreferencevmEmployeeChangedByUser(object sender, EventArgs e) => Refilter();
		protected void OnComboboxDateTypeChangedByUser(object sender, EventArgs e) => Refilter();
		protected void OnDateperiodpickerDateFilterPeriodChangedByUser(object sender, EventArgs e) => Refilter();

		protected void OnButtonExpiredClicked(object sender, EventArgs e)
		{
			Filter.SetDatePeriod(DateTime.Now.AddDays(-15), DateTime.Now.AddDays(-1));
			Refilter();
		}

		protected void OnButtonTodayClicked(object sender, EventArgs e)
		{
			Filter.SetDatePeriod(DateTime.Now, DateTime.Now);
			Refilter();
		}

		protected void OnButtonTomorrowClicked(object sender, EventArgs e)
		{
			Filter.SetDatePeriod(DateTime.Now.AddDays(1), DateTime.Now.AddDays(1));
			Refilter();
		}

		protected void OnButtonThisWeekClicked(object sender, EventArgs e)
		{
			Filter.SetDatePeriod(0);
			Refilter();
		}

		protected void OnButtonNextWeekClicked(object sender, EventArgs e)
		{
			Filter.SetDatePeriod(1);
			Refilter();
		}
	}
}
