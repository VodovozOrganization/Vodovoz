using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExtraBottleReport : Gtk.Bin, IParametersWidget
	{
		public ExtraBottleReport()
		{
			this.Build();
			datePeriodPicker.StartDate = DateTime.Now.AddMonths(-2);
			datePeriodPicker.EndDate = DateTime.Now;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по пересданной таре водителями";

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
				Identifier = "Bottles.ExtraBottlesReport",
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
