using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QSWidgetLib;
using QS.Dialog.GtkUI;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ZeroDebtClientReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public ZeroDebtClientReport(ReportFactory reportFactory)
		{
			this.Build();
			ydateperiodpicker.StartDate = DateTime.Now.Date;
			ydateperiodpicker.EndDate = DateTime.Now.Date;
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по нулевому долгу клиента";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "startDate", ydateperiodpicker.StartDate },
				{ "endDate", ydateperiodpicker.EndDate }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Client.ZeroDebtClient";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}
