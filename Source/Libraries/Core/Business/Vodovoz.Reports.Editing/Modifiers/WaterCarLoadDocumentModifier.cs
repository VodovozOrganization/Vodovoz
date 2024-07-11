using System.Collections.Generic;
using Vodovoz.Reports.Editing.ModifierActions;

namespace Vodovoz.Reports.Editing.Modifiers
{
	public class WaterCarLoadDocumentModifier : ReportModifierBase
	{
		private const string _dataTableName = "TableData";

		public void Setup()
		{
			AddActions(CopyTableAndPlaceToPositionActions(_dataTableName, "TableDataNew", 0, 670));
		}

		private static IEnumerable<ModifierAction> CopyTableAndPlaceToPositionActions(string sourceTableName, string newTableName, double newTableLeft, double newTableTop)
		{
			var actions = new List<ModifierAction>
			{
				CopyTableAction(sourceTableName, newTableName),
				SetTablePositionAction(sourceTableName, newTableLeft, newTableTop),
				AddOrderEqualTableFilterAction(sourceTableName, "150")
			};

			return actions;
		}

		private static ModifierAction CopyTableAction(string sourceTableName, string newTableName)
		{
			return new CopyTable(sourceTableName, newTableName);
		}

		private static ModifierAction RemoveTableAction(string tableName)
		{
			return new RemoveTable(tableName);
		}

		private static ModifierAction SetTablePositionAction(string elementName, double leftPositionInPt, double topPositionInPt)
		{
			return new SetTablePosition(elementName, leftPositionInPt, topPositionInPt);
		}

		private static ModifierAction MoveTableDownAction(string elementName, double offsetInPt)
		{
			return new MoveTableDown(elementName, offsetInPt);
		}

		private static ModifierAction RenameTableAction(string elementOldName, string elementNewName)
		{
			return new RenameTable(elementOldName, elementNewName);
		}

		private static ModifierAction AddOrderEqualTableFilterAction(string tableName, string value)
		{
			return AddTableFilterAction(tableName, "={amount}", "Equal", value);
		}

		private static ModifierAction AddOrderNotEqualTableFilterAction(string tableName, string value)
		{
			return AddTableFilterAction(tableName, "={amount}", "NotEqual", value);
		}

		private static ModifierAction AddTableFilterAction(string tableName, string expression, string @operator, string value)
		{
			return new AddTableFilter(tableName, expression, @operator, value);
		}
	}
}
