using System;
using System.Collections.Generic;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderIncorrectPrices : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public OrderIncorrectPrices()
		{
			this.Build();
			dateperiodpicker.StartDate = new DateTime(DateTime.Now.Year, 01, 01);
			dateperiodpicker.EndDate = DateTime.Now.Date;
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

			return new ReportInfo {
				Identifier = "Orders.OrdersIncorrectPrices",
				UseUserVariables = true,
				Parameters = parameters
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

		protected void OnCheckbutton1Toggled(object sender, EventArgs e)
		{
			dateperiodpicker.Sensitive = !checkbutton1.Active;
		}
	}
}
