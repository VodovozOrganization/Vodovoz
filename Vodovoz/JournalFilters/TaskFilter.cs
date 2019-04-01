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
				if(StartDate != null) dateperiodpickerFilter.StartDate = StartDate.Value;
				if(StartDate != null) dateperiodpickerFilter.EndDate = EndDate.Value;
			};
			EmployeesVM employeeVM = new EmployeesVM();
			employeeVM.Filter.RestrictCategory = EmployeeCategory.office;
			entryreferencevmEmployee.RepresentationModel = employeeVM;
		}

		public bool HideCompleted { get { return checkbuttonHideCompleted.Active; } }

		public Employee Employee { get { return entryreferencevmEmployee.Subject as Employee; } }

		public DateTime? StartDate;

		public DateTime? EndDate;

		protected void OnButtonExpiredClicked(object sender, EventArgs e)
		{
			StartDate = DateTime.Now.AddDays(-15);
			EndDate = DateTime.Now;
			FilterChanged?.Invoke();
		}

		protected void OnButtonTodayClicked(object sender, EventArgs e)
		{
			StartDate = DateTime.Now.Date;
			EndDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
			FilterChanged?.Invoke();
		}

		protected void OnButtonTomorrowClicked(object sender, EventArgs e)
		{
			StartDate = DateTime.Now.Date.AddDays(1);
			EndDate = DateTime.Now.Date.AddDays(1).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
			FilterChanged?.Invoke();
		}

		protected void OnButtonThisWeekClicked(object sender, EventArgs e)
		{
			DateHelper.GetWeekPeriod(out StartDate, out EndDate, 0);
			FilterChanged?.Invoke();
		}
	
		protected void OnButtonNextWeekClicked(object sender, EventArgs e)
		{
			DateHelper.GetWeekPeriod(out StartDate, out EndDate, 1);
			FilterChanged?.Invoke();
		}

		protected void OnDateperiodpickerFilterPeriodChangedByUser(object sender, EventArgs e)
		{
			StartDate = dateperiodpickerFilter.StartDateOrNull;
			EndDate = dateperiodpickerFilter.EndDateOrNull;
			FilterChanged?.Invoke();
		}

		protected void OnEntryreferencevmEmployeeChangedByUser(object sender, EventArgs e) => FilterChanged?.Invoke();

		protected void OnCheckbuttonHideCompletedToggled(object sender, EventArgs e) => FilterChanged?.Invoke();
	}
}
