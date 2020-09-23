using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashBookReport : SingleUoWWidgetBase, IParametersWidget
	{
		private string reportPath = "Wages.CashBook";
		public CashBookReport()
		{
			this.Build();
			dateperiodpicker.StartDate = DateTime.Now.Date;
			dateperiodpicker.EndDate = DateTime.Now.Date;
			buttonCreateRepot.Clicked += OnButtonCreateRepotClicked;
		}
		
		private ReportInfo GetReportInfo()
		{
			string startDate = $"{dateperiodpicker.StartDate:yyyy-MM-dd}";
			string endDate = $"{dateperiodpicker.EndDate:yyyy-MM-dd}";

			var parameters = new Dictionary<string, object> {
				{ "StartDate", startDate },
				{ "EndDate", endDate }
			};

			return new ReportInfo {
				Identifier = reportPath,
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}
		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public string Title => "Кассовая книга";
	}
}
