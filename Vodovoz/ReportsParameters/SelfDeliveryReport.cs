using System;
using System.Collections.Generic;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelfDeliveryReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public SelfDeliveryReport()
		{
			this.Build();
			ydatepicker.Date = DateTime.Now.Date;
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
				return "Отчет по самовывозу";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Orders.SelfDeliveryReport",
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
