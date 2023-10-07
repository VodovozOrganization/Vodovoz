using System.Collections.Generic;
using Vodovoz.Reports.Editing.ModifierActions;

namespace Vodovoz.Reports.Editing.Modifiers.CashFlowDetailReports
{
	public class CashFlowDetailReportModifier : ReportModifierBase
	{
		private const string _incomesIdentifier = "Incomes";
		private const string _mlAtDaysIdentifier = "MLatDays";
		private const string _incomeReturnsIdentifier = "IncomeReturns";
		private const string _expensesIdentifier = "Expenses";
		private const string _advancesIdentifier = "Advances";
		private const string _advanceReportsIdentifier = "AdvanceReports";
		private const string _unclosedAdvancesIdentifier = "UnclosedAdvances";
		
		public void Setup(ReportParts reportPart)
		{
			IEnumerable<ModifierAction> actions;

			switch(reportPart)
			{
				case ReportParts.IncomeAll:
					actions = GetIncomeAllActions();
					break;
				case ReportParts.Income:
					actions = GetIncomeActions();
					break;
				case ReportParts.IncomeReturn:
					actions = GetIncomeReturnActions();
					break;
				case ReportParts.ExpenseAll:
					actions = GetExpenseAllActions();
					break;
				case ReportParts.Expense:
					actions = GetExpenseActions();
					break;
				case ReportParts.Advance:
					actions = GetAdvanceActions();
					break;
				case ReportParts.AdvanceReport:
					actions = GetAdvanceReportActions();
					break;
				case ReportParts.UnclosedAdvance:
					actions = GetUnclosedAdvanceActions();
					break;
				default:
					return;
			}

			foreach(var action in actions)
			{
				AddAction(action);
			}
		}

		private static IEnumerable<ModifierAction> GetIncomeAllActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleAction(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetIncomeActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomeReturnsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetIncomeReturnActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetExpenseAllActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomeReturnsIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetExpenseActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomeReturnsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetAdvanceActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomeReturnsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetAdvanceReportActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomeReturnsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetUnclosedAdvanceActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_incomeReturnsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleAction(_advanceReportsIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> RemoveTableWithTextboxTitleAction(string identifier)
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(identifier),
				RemoveTextboxAction(identifier)
			};
		}

		private static ModifierAction RemoveTableAction(string identifier)
		{
			return new RemoveTable($"Table{identifier}");
		}

		private static ModifierAction RemoveTextboxAction(string identifier)
		{
			return new RemoveTextbox($"Textbox{identifier}");
		}
	}
}
