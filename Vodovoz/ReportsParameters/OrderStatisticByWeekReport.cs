using System;
using System.Collections.Generic;
using QS.Report;
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

		public string Title => "Статистика заказов по дням недели";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		public object EntityObject => null;

		void OnUpdate(bool hide = false) => 
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		protected void OnButtonRunClicked(object sender, EventArgs e) => OnUpdate(true);

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Logistic.OrderStatisticByWeek",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate.AddDays(1).AddTicks(-1) },
				}
			};
		}

		protected void OnDateperiodpickerPeriodChanged(object sender, EventArgs e) => 
			buttonRun.Sensitive = dateperiodpicker.StartDateOrNull.HasValue && dateperiodpicker.EndDateOrNull.HasValue;
	}
}
