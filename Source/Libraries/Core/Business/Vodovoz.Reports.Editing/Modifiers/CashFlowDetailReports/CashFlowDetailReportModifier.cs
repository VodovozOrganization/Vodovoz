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

		private const double _tableHeight = 110.0;
		private const double _firstTableTopPositionValue = 30.0;
		private const double _firstTextboxTopPositionValue = 7.0;
		
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

			actions.AddRange(RemoveTableWithTextboxTitleActions(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetIncomeActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomeReturnsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetIncomeReturnActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(MoveTableWithTextboxTitleActions(_incomeReturnsIdentifier, 1));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetExpenseAllActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomeReturnsIdentifier));
			actions.AddRange(MoveTableWithTextboxTitleActions(_expensesIdentifier, 1));
			actions.AddRange(MoveTableWithTextboxTitleActions(_advancesIdentifier, 2));
			actions.AddRange(MoveTableWithTextboxTitleActions(_advanceReportsIdentifier, 3));
			actions.AddRange(MoveTableWithTextboxTitleActions(_unclosedAdvancesIdentifier, 4));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetExpenseActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomeReturnsIdentifier));
			actions.AddRange(MoveTableWithTextboxTitleActions(_expensesIdentifier, 1));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetAdvanceActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomeReturnsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_expensesIdentifier));
			actions.AddRange(MoveTableWithTextboxTitleActions(_advancesIdentifier, 1));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advanceReportsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetAdvanceReportActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomeReturnsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advancesIdentifier));
			actions.AddRange(MoveTableWithTextboxTitleActions(_advanceReportsIdentifier, 1));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_unclosedAdvancesIdentifier));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetUnclosedAdvanceActions()
		{
			var actions = new List<ModifierAction>();

			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomesIdentifier));
			actions.Add(RemoveTableAction(_mlAtDaysIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_incomeReturnsIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_expensesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advancesIdentifier));
			actions.AddRange(RemoveTableWithTextboxTitleActions(_advanceReportsIdentifier));
			actions.AddRange(MoveTableWithTextboxTitleActions(_unclosedAdvancesIdentifier, 1));

			return actions;
		}

		private static IEnumerable<ModifierAction> RemoveTableWithTextboxTitleActions(string identifier)
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(identifier),
				RemoveTextboxAction(identifier)
			};
		}

		private static IEnumerable<ModifierAction> MoveTableWithTextboxTitleActions(string identifier, int tableOrdinalNumber)
		{
			var textboxLeftPosition = 0;
			var textboxTopPosition = (tableOrdinalNumber - 1) * _tableHeight + _firstTextboxTopPositionValue;

			var tableLeftPosition = 0;
			var tableTopPosition = (tableOrdinalNumber - 1) * _tableHeight + _firstTableTopPositionValue;

			return new List<ModifierAction>
			{
				SetTextboxPositionAction(identifier, textboxLeftPosition, textboxTopPosition),
				SetTablePositionAction(identifier, tableLeftPosition, tableTopPosition)
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

		private static ModifierAction SetTextboxPositionAction(string identifier, double left, double top)
		{
			return new SetTextboxPosition($"Textbox{identifier}", left, top);
		}

		private static ModifierAction SetTablePositionAction(string identifier, double left, double top)
		{
			return new SetTablePosition($"Table{identifier}", left, top);
		}
	}
}
