using System;
using System.Collections.Generic;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderStatisticByWeekReport : Gtk.Bin, IParametersWidget
	{
		public OrderStatisticByWeekReport()
		{
			this.Build();
			dateperiodpicker.StartDate = new DateTime(DateTime.Today.Year, 1, 1);
			dateperiodpicker.EndDate = DateTime.Today;
		}

		#region IParametersWidget implementation

		public string Title {
			get {
				return "Статистика заказов по дням недели";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		public object EntityObject {
			get {
				return null;
			}
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Orders.OrderStatisticByWeek",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate },
				}
			};
		}

		protected void OnDateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			buttonRun.Sensitive = dateperiodpicker.StartDateOrNull.HasValue && dateperiodpicker.EndDateOrNull.HasValue;
		}
	}
}
