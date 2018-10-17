using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeBalanceReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public IncomeBalanceReport()
		{
			this.Build();
			dateperiodpicker.StartDate = DateTime.Now.Date;
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
				return "Отчет по прибыли";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			string startDate = String.Format("{0:yyyy-MM-dd}", dateperiodpicker.StartDate);
			string endDate = String.Format("{0:yyyy-MM-dd}", dateperiodpicker.EndDate);

			var parameters = new Dictionary<string, object>();
			parameters.Add("StartDate", startDate);
			parameters.Add("EndDate", endDate);

			return new ReportInfo {
				Identifier = "Sales.IncomeBalance",
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
	}
}
