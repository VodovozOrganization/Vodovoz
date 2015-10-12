using System;
using QSReport;
using System.Collections.Generic;

namespace Vodovoz.Reports
{
	public partial class CashFlow : Gtk.Bin, IParametersWidget
	{
		public CashFlow ()
		{
			this.Build ();
		}

		#region IParametersWidget implementation

		public string Title {
			get { return "Доходы и расходы";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;
		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null)
			{
				LoadReport (this, new LoadReportEventArgs (GetReportInfo (), hide));
			}
		}

		protected void OnButtonRunClicked (object sender, EventArgs e)
		{
			OnUpdate (true);
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = checkDetail.Active ? "Cash.CashFlowDetail" : "Cash.CashFlow",
				Parameters = new Dictionary<string, object> {
					{"StartDate", dateperiodpicker1.StartDateOrNull.Value},
					{"EndDate", dateperiodpicker1.EndDateOrNull.Value}
				}
			};
		}

		protected void OnDateperiodpicker1PeriodChanged (object sender, EventArgs e)
		{
			buttonRun.Sensitive = dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null;
		}
	}
}

