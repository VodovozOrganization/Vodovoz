using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class NonClosedRLByPeriodReport : Gtk.Bin, IParametersWidget
	{
		public NonClosedRLByPeriodReport()
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ybtnCreateReport.Clicked += (sender, e) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo()));
			dateperiodpicker.SetPeriod(DateTime.Today.AddMonths(-1), DateTime.Today);
			dateperiodpicker.PeriodChangedByUser += OnDateperiodpickerPeriodChangedByUser;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по незакрытым МЛ за период";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "Logistic.NonClosedRLByPeriodReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
					{ "create_date", DateTime.Now },
					{ "delay", yspinbtnDelay.ValueAsInt }
				}
			};
		}

		private void OnDateperiodpickerPeriodChangedByUser(object sender, EventArgs e)
		{
			ybtnCreateReport.Sensitive = dateperiodpicker.StartDateOrNull.HasValue && dateperiodpicker.EndDateOrNull.HasValue;
		}
	}
}
