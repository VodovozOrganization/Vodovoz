using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters.Bookkeeping
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyCloseDeliveryReport : Gtk.Bin , IParametersWidget
	{
		public CounterpartyCloseDeliveryReport()
		{
			this.Build();
			datePeriodPicker.StartDate = DateTime.Now.AddMonths(-1);
			datePeriodPicker.EndDate = DateTime.Now;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет закрытых отгрузок";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var reportInfo = new ReportInfo {
				Identifier = "Bookkeeping.CloseCounterpartyDelivery",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", datePeriodPicker.StartDateOrNull},
					{ "end_date", datePeriodPicker.EndDateOrNull},
				}
			};
			return reportInfo;
		}
	}
}
