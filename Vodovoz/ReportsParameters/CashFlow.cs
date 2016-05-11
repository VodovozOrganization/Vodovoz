using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Binding;
using Gtk;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Cash;
using Vodovoz.Repository.Cash;

namespace Vodovoz.Reports
{
	public partial class CashFlow : Gtk.Bin, IParametersWidget
	{
		ExpenseCategory allItem = new ExpenseCategory{
			Name = "Все"
		};

		public CashFlow ()
		{
			this.Build ();
			var uow = UnitOfWorkFactory.CreateWithoutRoot ();
			comboPart.ItemsEnum = typeof(ReportParts);
			comboIncomeCategory.ItemsList = CategoryRepository.IncomeCategories (uow);
			comboExpenseCategory.Sensitive = comboIncomeCategory.Sensitive = false;

			var recurciveConfig = OrmMain.GetObjectDescription<ExpenseCategory>().TableView.RecursiveTreeConfig;
			var list = CategoryRepository.ExpenseCategories(uow);
			list.Insert(0, allItem);
			var model = recurciveConfig.CreateModel((IList)list);
			comboExpenseCategory.Model = model.Adapter;
			comboExpenseCategory.PackStart(new CellRendererText(), true);
			comboExpenseCategory.SetCellDataFunc(comboExpenseCategory.Cells[0], HandleCellLayoutDataFunc);
			comboExpenseCategory.SetActiveIter(model.IterFromNode(allItem));
		}

		void HandleCellLayoutDataFunc (Gtk.CellLayout cell_layout, CellRenderer cell, Gtk.TreeModel tree_model, Gtk.TreeIter iter)
		{
			string text = DomainHelper.GetObjectTilte(tree_model.GetValue(iter, 0));
			(cell as CellRendererText).Text = text;
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
				else if (comboPart.SelectedItem.Equals (ReportParts.UnclosedAdvance))
					ReportName = "Cash.CashFlowDetailUnclosedAdvance";
				else
					throw new InvalidOperationException ("Неизвестный раздел.");
			} else
				ReportName = "Cash.CashFlow";

			var inCat = 
				comboIncomeCategory.SelectedItem == null
				|| comboIncomeCategory.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All)
				? -1
				: (comboIncomeCategory.SelectedItem as IncomeCategory).Id;

			TreeIter iter;
			comboExpenseCategory.GetActiveIter(out iter);
			var exCategory = (ExpenseCategory)comboExpenseCategory.Model.GetValue(iter, 0);
			bool exCategorySelected = exCategory != allItem;
			var ids = new List<int>();
			if (exCategorySelected)
				FineIds(ids, exCategory);
			else
				ids.Add(0); //Add fake value
			
			return new ReportInfo {
				Identifier = ReportName,
				Parameters = new Dictionary<string, object> {
					{ "StartDate", dateperiodpicker1.StartDateOrNull.Value },
					{ "EndDate", dateperiodpicker1.EndDateOrNull.Value },
					{ "IncomeCategory", inCat },
					{ "ExpenseCategory", ids },
					{ "ExpenseCategoryUsed", exCategorySelected ? 1 : 0 }
				}
			};
		}

		private void FineIds(IList<int> result, ExpenseCategory cat)
		{
			result.Add(cat.Id);
			if (cat.Childs == null)
				return;

			foreach(var childCat in cat.Childs)
			{
				FineIds(result, childCat);
			}
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
			if (comboPart.SelectedItem.Equals (Gamma.Widgets.SpecialComboState.All)
				|| comboPart.SelectedItem.Equals (ReportParts.IncomeAll))
				comboExpenseCategory.Sensitive = comboIncomeCategory.Sensitive = true;
			else if (comboPart.SelectedItem.Equals (ReportParts.Income)) {
				comboExpenseCategory.Sensitive = false;
				comboIncomeCategory.Sensitive = true;
			} else if (comboPart.SelectedItem.Equals (ReportParts.ExpenseAll)
			           || comboPart.SelectedItem.Equals (ReportParts.Expense)
			           || comboPart.SelectedItem.Equals (ReportParts.Advance)
			           || comboPart.SelectedItem.Equals (ReportParts.AdvanceReport)
					|| comboPart.SelectedItem.Equals (ReportParts.UnclosedAdvance)
				|| comboPart.SelectedItem.Equals (ReportParts.IncomeReturn)) {
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
			AdvanceReport,
			[Display (Name = "Незакрытые авансы")]
			UnclosedAdvance
		}
	}
}

