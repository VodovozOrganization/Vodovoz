using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SendedEmailsReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public SendedEmailsReport()
		{
			this.Build();
			ydateperiodpicker.StartDate = DateTime.Now;
			ydateperiodpicker.EndDate = DateTime.Now;
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
				return "Отчет по отправке счетов";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Orders.SendedBillsReport",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", ydateperiodpicker.StartDate },
					{ "endDate", ydateperiodpicker.EndDate }
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
