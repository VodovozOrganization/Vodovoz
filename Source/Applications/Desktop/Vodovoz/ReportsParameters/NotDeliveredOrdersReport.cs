using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NotDeliveredOrdersReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public NotDeliveredOrdersReport(ReportFactory reportFactory)
		{
			this.Build();
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			dateperiodpicker1.StartDate = DateTime.Now.Date;
			dateperiodpicker1.EndDate = DateTime.Now.Date;
		}

		#region IParametersWidget implementation

		public string Title {
			get {
				return "Отчет по недовозам";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "startDate", dateperiodpicker1.StartDateOrNull },
				{ "endDate", dateperiodpicker1.EndDateOrNull },
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Orders.NotDeliveredOrders";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}
	}
}
