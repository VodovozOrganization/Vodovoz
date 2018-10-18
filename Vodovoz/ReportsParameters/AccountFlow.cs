using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Report;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Cash;
using Vodovoz.Repository.Cash;

namespace Vodovoz.Reports
{
	public partial class AccountFlow : Gtk.Bin, IParametersWidget
	{
		public AccountFlow ()
		{
			this.Build ();
			var uow = UnitOfWorkFactory.CreateWithoutRoot ();
			comboPart.ItemsEnum = typeof(ReportParts);
			comboExpenseCategory.ItemsList = CategoryRepository.ExpenseCategories (uow);
			comboExpenseCategory.SelectedItem = Gamma.Widgets.SpecialComboState.All;
			comboIncomeCategory.ItemsList = CategoryRepository.IncomeCategories (uow);
			comboIncomeCategory.SelectedItem = Gamma.Widgets.SpecialComboState.All;

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
			if (comboPart.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All))
				ReportName = "Cash.AccountFlowDetail";
			else if (comboPart.SelectedItem.Equals (ReportParts.Income))
				ReportName = "Cash.AccountFlowDetailIncome";
			else if (comboPart.SelectedItem.Equals (ReportParts.Expense))
				ReportName = "Cash.AccountFlowDetailExpense";
			else
				throw new InvalidOperationException ("Неизвестный раздел.");
			var inCat = 
				comboIncomeCategory.SelectedItem == null
				|| comboIncomeCategory.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All)
				? -1
				: (comboIncomeCategory.SelectedItem as IncomeCategory).Id;
			var exCat = 
				comboExpenseCategory.SelectedItem == null
				|| comboExpenseCategory.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All)
				? -1
				: (comboExpenseCategory.SelectedItem as ExpenseCategory).Id;
			
			return new ReportInfo {
				Identifier = ReportName,
				Parameters = new Dictionary<string, object> {
					{ "StartDate", dateperiodpicker1.StartDateOrNull.Value },
					{ "EndDate", dateperiodpicker1.EndDateOrNull.Value },
					{ "IncomeCategory", inCat },
					{ "ExpenseCategory", exCat }
				}
			};
		}

		protected void OnDateperiodpicker1PeriodChanged (object sender, EventArgs e)
		{
			buttonRun.Sensitive = dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null;
		}

		protected void OnComboPartEnumItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			if (comboPart.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All))
				comboExpenseCategory.Sensitive = comboIncomeCategory.Sensitive = true;
			else if (comboPart.SelectedItem.Equals (ReportParts.Income)) {
				comboExpenseCategory.Sensitive = false;
				comboIncomeCategory.Sensitive = true;
			} else if (comboPart.SelectedItem.Equals (ReportParts.Expense)) {
				comboIncomeCategory.Sensitive = false;
				comboExpenseCategory.Sensitive = true;
			} else
				throw new InvalidOperationException ("Неизвестный раздел.");
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

