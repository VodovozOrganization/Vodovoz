using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NonClosedRLByPeriodReport : Gtk.Bin, IParametersWidget
	{
		public NonClosedRLByPeriodReport()
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ybtnCreateReport.Clicked += (sender, e) => OnUpdate();
			dateperiodpicker.SetPeriod(DateTime.Today.AddMonths(-1), DateTime.Today);
			dateperiodpicker.PeriodChangedByUser += OnDateperiodpickerPeriodChangedByUser;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по незакрытым МЛ за период";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Logistic.NonClosedRLByPeriodReport",
				Parameters = new Dictionary<string, object>
				{
					{"start_date", dateperiodpicker.StartDateOrNull.Value},
					{"end_date", dateperiodpicker.EndDateOrNull.Value.AddHours(23).AddMinutes(59).AddSeconds(59)},
					{"create_date", DateTime.Now},
					{"delay", yspinbtnDelay.ValueAsInt}
				}
			};
		}

		void OnUpdate(bool hide = false) =>
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		void OnDateperiodpickerPeriodChangedByUser(object sender, EventArgs e)
		{
			ybtnCreateReport.Sensitive = dateperiodpicker.StartDateOrNull.HasValue 
											&& dateperiodpicker.EndDateOrNull.HasValue;
		}
	}
}
