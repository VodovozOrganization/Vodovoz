using System;
using QS.Utilities;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TaskFilter : Gtk.Bin
	{
		public event Action FilterChanged;

		public TaskFilter()
		{
			this.Build();
			FilterChanged += () => {
				if(StartActivePerionDate != null) {
					dateperiodpickerDeadlineFilter.StartDate = StartActivePerionDate.Value;
					dateperiodpickerDeadlineFilter.EndDate = EndActivePeriodDate.Value;
				}
				if(StartActivePerionDate != null) {
					dateperiodpickerCreateDateFilter.StartDate = StartTaskCreateDate.Value;
					dateperiodpickerCreateDateFilter.EndDate = EndActivePeriodDate.Value;
				} 
			};
			EmployeesVM employeeVM = new EmployeesVM();
			employeeVM.Filter.RestrictCategory = EmployeeCategory.office;
			entryreferencevmEmployee.RepresentationModel = employeeVM;
		}

		public bool HideCompleted { get { return checkbuttonHideCompleted.Active; } }

		public Employee Employee { get { return entryreferencevmEmployee.Subject as Employee; } }

		public DateTime? StartActivePerionDate;

		public DateTime? EndActivePeriodDate;

		public DateTime? StartTaskCreateDate;

		public DateTime? EndTaskCreateDate;

		protected void OnButtonExpiredClicked(object sender, EventArgs e)
		{
			StartActivePerionDate = DateTime.Now.AddDays(-15);
			EndActivePeriodDate = DateTime.Now;
			FilterChanged?.Invoke();
		}

		protected void OnButtonTodayClicked(object sender, EventArgs e)
		{
			StartActivePerionDate = DateTime.Now.Date;
			EndActivePeriodDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
			FilterChanged?.Invoke();
		}

		protected void OnButtonTomorrowClicked(object sender, EventArgs e)
		{
			StartActivePerionDate = DateTime.Now.Date.AddDays(1);
			EndActivePeriodDate = DateTime.Now.Date.AddDays(1).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
			FilterChanged?.Invoke();
		}

		protected void OnButtonThisWeekClicked(object sender, EventArgs e)
		{
			DateHelper.GetWeekPeriod(out StartActivePerionDate, out EndActivePeriodDate, 0);
			FilterChanged?.Invoke();
		}
	
		protected void OnButtonNextWeekClicked(object sender, EventArgs e)
		{
			DateHelper.GetWeekPeriod(out StartActivePerionDate, out EndActivePeriodDate, 1);
			FilterChanged?.Invoke();
		}

		protected void OnDateperiodpickerDeadlineFilterPeriodChangedByUser(object sender, EventArgs e)
		{
			StartTaskCreateDate = null;
			EndTaskCreateDate = null;
			dateperiodpickerCreateDateFilter.StartDate = dateperiodpickerCreateDateFilter.EndDate = DateTime.MinValue;
			StartActivePerionDate = dateperiodpickerDeadlineFilter.StartDateOrNull;
			EndActivePeriodDate = dateperiodpickerDeadlineFilter.EndDateOrNull?.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
			FilterChanged?.Invoke();
		}

		protected void OnDateperiodpickerCreateDateStartDateChanged(object sender, EventArgs e)
		{
			StartActivePerionDate  = null;
			EndActivePeriodDate = null;
			dateperiodpickerDeadlineFilter.StartDate = dateperiodpickerDeadlineFilter.EndDate = DateTime.MinValue;
			StartTaskCreateDate = dateperiodpickerCreateDateFilter.StartDateOrNull;
			EndTaskCreateDate = dateperiodpickerCreateDateFilter.EndDateOrNull?.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
			FilterChanged?.Invoke();
		}

		protected void OnEntryreferencevmEmployeeChangedByUser(object sender, EventArgs e) => FilterChanged?.Invoke();

		protected void OnCheckbuttonHideCompletedToggled(object sender, EventArgs e) => FilterChanged?.Invoke();
	}
}
