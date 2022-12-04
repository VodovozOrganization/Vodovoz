using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.Report;
using QSReport;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsFromBankClientFinDepartmentReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public PaymentsFromBankClientFinDepartmentReport(ReportFactory reportFactory)
		{
			this.Build();
			btnCreateReport.Clicked += (sender, e) => OnUpdate(true);
			btnCreateReport.Sensitive = false;
			daterangepicker.PeriodChangedByUser += Daterangepicker_PeriodChangedByUser;
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
		}

		void Daterangepicker_PeriodChangedByUser(object sender, EventArgs e) => 
			btnCreateReport.Sensitive = daterangepicker.EndDateOrNull.HasValue && daterangepicker.StartDateOrNull.HasValue;


		#region IParametersWidget implementation

		public string Title => "Отчет по оплатам (ФО)";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", daterangepicker.StartDate },
				{ "end_date", daterangepicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Payments.PaymentsFromBankClientFinDepartmentReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
	}
}
