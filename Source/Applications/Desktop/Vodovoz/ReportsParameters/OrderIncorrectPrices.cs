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
	public partial class OrderIncorrectPrices : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public OrderIncorrectPrices(ReportFactory reportFactory)
		{
			this.Build();
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			dateperiodpicker.StartDate = DateTime.Now.Date;
			dateperiodpicker.EndDate = DateTime.Now.Date;
		}

		#region IParametersWidget implementation

		public string Title {
			get {
				return "Отчет по некорректным ценам";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			string dateFrom = "";
			string dateTo = "";
			if(dateperiodpicker.Sensitive) {
				dateFrom = String.Format("{0:yyyy-MM-dd}", dateperiodpicker.StartDate);
				dateTo = String.Format("{0:yyyy-MM-dd}", dateperiodpicker.EndDate);
			}
			var parameters = new Dictionary<string, object>();
			parameters.Add("dateFrom", dateFrom);
			parameters.Add("dateTo", dateTo);

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Orders.OrdersIncorrectPrices";
			reportInfo.UseUserVariables = true;
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

		protected void OnCheckbutton1Toggled(object sender, EventArgs e)
		{
			dateperiodpicker.Sensitive = !checkbutton1.Active;
		}
	}
}
