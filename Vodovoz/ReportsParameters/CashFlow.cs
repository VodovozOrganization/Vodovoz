using System;
using QSReport;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Repository.Cash;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Reports
{
	public partial class CashFlow : Gtk.Bin, IParametersWidget
	{
		public CashFlow ()
		{
			this.Build ();
			var uow = UnitOfWorkFactory.CreateWithoutRoot ();
			comboPart.ItemsEnum = typeof(ReportParts);
			comboExpenseCategory.ItemsList = CategoryRepository.ExpenseCategories (uow);
			comboIncomeCategory.ItemsList = CategoryRepository.IncomeCategories (uow);
			comboExpenseCategory.Sensitive = comboIncomeCategory.Sensitive = false;
		}

		#region IParametersWidget implementation

		public string Title {
			get {
				return "Доходы и расходы";
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
			if (checkDetail.Active) {
				if (comboPart.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All))
					ReportName = "Cash.CashFlowDetail";
				else if (comboPart.SelectedItem.Equals (ReportParts.IncomeAll))
					ReportName = "Cash.CashFlowDetailIncomeAll";
				else if (comboPart.SelectedItem.Equals (ReportParts.Income))
					ReportName = "Cash.CashFlowDetailIncome";
				else if (comboPart.SelectedItem.Equals (ReportParts.IncomeReturn))
					ReportName = "Cash.CashFlowDetailIncomeReturn";
				else if (comboPart.SelectedItem.Equals (ReportParts.ExpenseAll))
					ReportName = "Cash.CashFlowDetailExpenseAll";
				else if (comboPart.SelectedItem.Equals (ReportParts.Expense))
					ReportName = "Cash.CashFlowDetailExpense";
				else if (comboPart.SelectedItem.Equals (ReportParts.Advance))
					ReportName = "Cash.CashFlowDetailAdvance";
				else if (comboPart.SelectedItem.Equals (ReportParts.AdvanceReport))
					ReportName = "Cash.CashFlowDetailAdvanceReport";
				else
					throw new InvalidOperationException ("Неизвестный раздел.");
			} else
				ReportName = "Cash.CashFlow";

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

		protected void OnCheckDetailToggled (object sender, EventArgs e)
		{
			comboPart.Sensitive = comboExpenseCategory.Sensitive =
				comboIncomeCategory.Sensitive = checkDetail.Active;
		}

		protected void OnComboPartEnumItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			if (comboPart.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All))
				comboExpenseCategory.Sensitive = comboIncomeCategory.Sensitive = true;
			else if (comboPart.SelectedItem.Equals (ReportParts.IncomeAll)
			         || comboPart.SelectedItem.Equals (ReportParts.Income)
			         || comboPart.SelectedItem.Equals (ReportParts.IncomeReturn)) {
				comboExpenseCategory.Sensitive = false;
				comboIncomeCategory.Sensitive = true;
			} else if (comboPart.SelectedItem.Equals (ReportParts.ExpenseAll)
			           || comboPart.SelectedItem.Equals (ReportParts.Expense)
			           || comboPart.SelectedItem.Equals (ReportParts.Advance)
			           || comboPart.SelectedItem.Equals (ReportParts.AdvanceReport)) {
				comboExpenseCategory.Sensitive = true;
				comboIncomeCategory.Sensitive = false;
			} else
				throw new InvalidOperationException ("Неизвестный раздел.");
		}

		enum ReportParts
		{
			[Display (Name = "Поступления суммарно")]
			IncomeAll,
			[Display (Name = "Приход")]
			Income,
			[Display (Name = "Сдача")]
			IncomeReturn,
			[Display (Name = "Расходы суммарно")]
			ExpenseAll,
			[Display (Name = "Расход")]
			Expense,
			[Display (Name = "Авансы")]
			Advance,
			[Display (Name = "Авансовые отчеты")]
			AdvanceReport
		}
	}
}

