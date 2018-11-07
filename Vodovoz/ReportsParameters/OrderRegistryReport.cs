using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderRegistryReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public OrderRegistryReport()
		{
			this.Build();
			ydatepicker.Date = DateTime.Now.Date;
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Реестр заказов";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Orders.OrderRegistryReport",
				Parameters = new Dictionary<string, object>
				{
					{ "date", ydatepicker.Date }
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
