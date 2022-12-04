using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class SuburbWaterPriceReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по ценам пригорода";
			}
		}


		public SuburbWaterPriceReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "report_date", ydatepicker.Date }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Sales.SuburbWaterPrice";
			reportInfo.ParameterDatesWithTime = false;
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}
