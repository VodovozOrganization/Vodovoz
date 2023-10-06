using System.Collections.Generic;
using Vodovoz.Reports.Editing.ModifierActions;

namespace Vodovoz.Reports.Editing.Modifiers.CashFlowDetailReports
{
	public class CashFlowDetailReportModifier : ReportModifierBase
	{
		private const string _tableIncomesName = "TableIncomes";
		private const string _tableMlAtDaysName = "TableMLatDays";
		private const string _tableIncomeReturnsName = "TableIncomeReturns";
		private const string _tableExpensesName = "TableExpenses";
		private const string _tableAdvancesName = "TableAdvances";
		private const string _tableAdvanceReportsName = "TableAdvanceReports";
		private const string _tableUnclosedAdvancesName = "TableUnclosedAdvances";
		
		public void Setup(ReportParts reportPart)
		{
			IEnumerable<ModifierAction> actions = null;

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
					break;
			}

			if(actions == null)
			{
				return;
			}

			foreach(var action in actions)
			{
				AddAction(action);
			}
		}

		private static IEnumerable<ModifierAction> GetIncomeAllActions()
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(_tableExpensesName),
				RemoveTableAction(_tableAdvancesName),
				RemoveTableAction(_tableAdvanceReportsName),
				RemoveTableAction(_tableUnclosedAdvancesName)
			};
		}

		private static IEnumerable<ModifierAction> GetIncomeActions()
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(_tableIncomeReturnsName),
				RemoveTableAction(_tableExpensesName),
				RemoveTableAction(_tableAdvancesName),
				RemoveTableAction(_tableAdvanceReportsName),
				RemoveTableAction(_tableUnclosedAdvancesName)
			};
		}

		private static IEnumerable<ModifierAction> GetIncomeReturnActions()
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(_tableIncomesName),
				RemoveTableAction(_tableMlAtDaysName),
				RemoveTableAction(_tableExpensesName),
				RemoveTableAction(_tableAdvancesName),
				RemoveTableAction(_tableAdvanceReportsName),
				RemoveTableAction(_tableUnclosedAdvancesName)
			};
		}

		private static IEnumerable<ModifierAction> GetExpenseAllActions()
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(_tableIncomesName),
				RemoveTableAction(_tableMlAtDaysName),
				RemoveTableAction(_tableIncomeReturnsName)
			};
		}

		private static IEnumerable<ModifierAction> GetExpenseActions()
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(_tableIncomesName),
				RemoveTableAction(_tableMlAtDaysName),
				RemoveTableAction(_tableIncomeReturnsName),
				RemoveTableAction(_tableAdvancesName),
				RemoveTableAction(_tableAdvanceReportsName),
				RemoveTableAction(_tableUnclosedAdvancesName)
			};
		}

		private static IEnumerable<ModifierAction> GetAdvanceActions()
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(_tableIncomesName),
				RemoveTableAction(_tableMlAtDaysName),
				RemoveTableAction(_tableIncomeReturnsName),
				RemoveTableAction(_tableExpensesName),
				RemoveTableAction(_tableAdvanceReportsName),
				RemoveTableAction(_tableUnclosedAdvancesName)
			};
		}

		private static IEnumerable<ModifierAction> GetAdvanceReportActions()
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(_tableIncomesName),
				RemoveTableAction(_tableMlAtDaysName),
				RemoveTableAction(_tableIncomeReturnsName),
				RemoveTableAction(_tableExpensesName),
				RemoveTableAction(_tableAdvancesName),
				RemoveTableAction(_tableUnclosedAdvancesName)
			};
		}

		private static IEnumerable<ModifierAction> GetUnclosedAdvanceActions()
		{
			return new List<ModifierAction>
			{
				RemoveTableAction(_tableIncomesName),
				RemoveTableAction(_tableMlAtDaysName),
				RemoveTableAction(_tableIncomeReturnsName),
				RemoveTableAction(_tableExpensesName),
				RemoveTableAction(_tableAdvancesName),
				RemoveTableAction(_tableAdvanceReportsName)
			};
		}

		private static ModifierAction RemoveTableAction(string tableName)
		{
			return new RemoveTable(tableName);
		}

	}
}
