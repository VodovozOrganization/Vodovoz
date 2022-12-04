using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersWithMinPriceLessThan : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public OrdersWithMinPriceLessThan(ReportFactory reportFactory)
		{
			this.Build();
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по заказам с минимальной ценой меньше 100р.";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Orders.OrdersWithMinPriceLessThan";
			reportInfo.Parameters = new Dictionary<string, object>();

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
