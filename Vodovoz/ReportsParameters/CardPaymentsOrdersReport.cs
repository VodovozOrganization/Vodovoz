using System;
using System.Collections.Generic;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CardPaymentsOrdersReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public CardPaymentsOrdersReport()
		{
			this.Build();
			ydateperiodpicker.StartDate = DateTime.Now.Date;
			ydateperiodpicker.EndDate = DateTime.Now.Date;
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject {
			get {
				return null;
			}
		}

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по оплатам по картам";

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Orders.CardPaymentsOrdersReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", ydateperiodpicker.StartDate },
					{ "end_date", ydateperiodpicker.EndDate }
				}
			};
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
