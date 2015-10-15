using System;
using QSReport;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Widgets;

namespace Vodovoz.Reports
{
	public partial class CashFlow : Gtk.Bin, IParametersWidget
	{
		public CashFlow ()
		{
			this.Build ();
			comboPart.ItemsEnum = typeof(ReportParts);
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
			string ReportName;
			if (checkDetail.Active) {
				if (comboPart.SelectedItem.Equals (SpecialComboState.All))
					ReportName = "Cash.CashFlowDetail";
				else if (comboPart.SelectedItem.Equals (ReportParts.Income))
					ReportName = "Cash.CashFlowDetailIncome";
				else if (comboPart.SelectedItem.Equals (ReportParts.Expense))
					ReportName = "Cash.CashFlowDetailExpense";
				else if (comboPart.SelectedItem.Equals (ReportParts.Advance))
					ReportName = "Cash.CashFlowDetailAdvance";
				else
					throw new InvalidOperationException ("Неизвестный раздел.");
			} else
				ReportName = "Cash.CashFlow";

			return new ReportInfo {
				Identifier = ReportName,
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

		protected void OnCheckDetailToggled (object sender, EventArgs e)
		{
			comboPart.Sensitive = checkDetail.Active;
		}

		enum ReportParts
		{
			[Display(Name = "Приход")]
			Income,
			[Display(Name = "Расход")]
			Expense,
			[Display(Name = "Авансы")]
			Advance
		}
	}
}

