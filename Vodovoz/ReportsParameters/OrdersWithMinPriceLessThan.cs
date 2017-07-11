using System;
using System.Collections.Generic;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersWithMinPriceLessThan : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public OrdersWithMinPriceLessThan()
		{
			this.Build();
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

		public string Title {
			get {
				return "Отчет по заказам с минимальной ценой меньше 130р.";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Orders.OrdersWithMinPriceLessThan",
				Parameters = new Dictionary<string, object>
				{
					
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
