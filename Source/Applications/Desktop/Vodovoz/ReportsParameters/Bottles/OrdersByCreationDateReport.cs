using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersByCreationDateReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public OrdersByCreationDateReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			pkrDate.Date = DateTime.Today;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по дате создания заказа";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null)
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "date", pkrDate.Date.ToString("yyyy-MM-dd") }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Bottles.OrdersByCreationDate";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}
	}
}
