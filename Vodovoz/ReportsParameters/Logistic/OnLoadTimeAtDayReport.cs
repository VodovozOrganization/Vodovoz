using System;
using System.Collections.Generic;
using QSProjectsLib;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class OnLoadTimeAtDayReport : Gtk.Bin, IParametersWidget
	{
		public OnLoadTimeAtDayReport()
		{
			this.Build();
			ydateAtDay.Date = DateTime.Today;
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Время погрузки на складе";
			}
		}

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Logistic.OnLoadTimeAtDay",
				Parameters = new Dictionary<string, object>
				{
					{ "date", ydateAtDay.Date },
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		protected void OnYdateAtDayDateChanged(object sender, EventArgs e)
		{
			buttonCreateReport.Sensitive = !ydateAtDay.IsEmpty;
		}
	}
}
