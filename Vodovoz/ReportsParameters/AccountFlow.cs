using System;
using QSReport;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Widgets;

namespace Vodovoz.Reports
{
	public partial class AccountFlow : Gtk.Bin, IParametersWidget
	{
		public AccountFlow ()
		{
			this.Build ();
			comboPart.ItemsEnum = typeof(ReportParts);
		}

		#region IParametersWidget implementation

		public string Title {
			get {
				return "Доходы и расходы (безнал)";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate (bool hide = false)
		{
			if (LoadReport != null) {
				LoadReport (this, new LoadReportEventArgs (GetReportInfo (), hide));
			}
		}

		protected void OnButtonRunClicked (object sender, EventArgs e)
		{
			OnUpdate (true);
		}

		private ReportInfo GetReportInfo ()
		{
			string ReportName;
			if (comboPart.SelectedItem.Equals (SpecialComboState.All))
				ReportName = "Cash.AccountFlowDetail";
			else if (comboPart.SelectedItem.Equals (ReportParts.Income))
				ReportName = "Cash.AccountFlowDetailIncome";
			else if (comboPart.SelectedItem.Equals (ReportParts.Expense))
				ReportName = "Cash.AccountFlowDetailExpense";
			else
				throw new InvalidOperationException ("Неизвестный раздел.");
			return new ReportInfo {
				Identifier = ReportName,
				Parameters = new Dictionary<string, object> {
					{ "StartDate", dateperiodpicker1.StartDateOrNull.Value },
					{ "EndDate", dateperiodpicker1.EndDateOrNull.Value }
				}
			};
		}

		protected void OnDateperiodpicker1PeriodChanged (object sender, EventArgs e)
		{
			buttonRun.Sensitive = dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null;
		}

		enum ReportParts
		{
			[Display (Name = "Приход")]
			Income,
			[Display (Name = "Расход")]
			Expense
		}
	}
}

