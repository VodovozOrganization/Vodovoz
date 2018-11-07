using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NotDeliveredOrdersReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public IUnitOfWork UoW { get; private set; }

		public NotDeliveredOrdersReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
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
			return new ReportInfo {
				Identifier = "Orders.NotDeliveredOrders",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", dateperiodpicker1.StartDateOrNull },
					{ "endDate", dateperiodpicker1.EndDateOrNull },
				}
			};
		}
	}
}
