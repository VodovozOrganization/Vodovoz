using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Orders;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExtraBottleReport : Gtk.Bin, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public ExtraBottleReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
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
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", datePeriodPicker.StartDateOrNull},
				{ "end_date", datePeriodPicker.EndDateOrNull},
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Bottles.ExtraBottlesReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}
	}
}
