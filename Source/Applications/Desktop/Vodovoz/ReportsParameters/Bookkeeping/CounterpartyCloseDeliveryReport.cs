using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Bookkeeping
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyCloseDeliveryReport : Gtk.Bin , IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public CounterpartyCloseDeliveryReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
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
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", datePeriodPicker.StartDateOrNull},
				{ "end_date", datePeriodPicker.EndDateOrNull},
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Bookkeeping.CloseCounterpartyDelivery";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}
	}
}
