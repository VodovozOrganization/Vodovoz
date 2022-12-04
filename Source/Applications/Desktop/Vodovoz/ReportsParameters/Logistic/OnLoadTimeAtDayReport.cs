using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class OnLoadTimeAtDayReport : Gtk.Bin, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public OnLoadTimeAtDayReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
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
			var parameters = new Dictionary<string, object>
			{
				{ "date", ydateAtDay.Date },
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Logistic.OnLoadTimeAtDay";
			reportInfo.Parameters = parameters;

			return reportInfo;
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
